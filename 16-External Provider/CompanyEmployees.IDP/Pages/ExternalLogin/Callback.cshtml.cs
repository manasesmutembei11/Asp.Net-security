using System.Security.Claims;
using CompanyEmployees.IDP.Entities;
using Duende.IdentityServer;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Test;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CompanyEmployees.IDP.Pages.ExternalLogin;

[AllowAnonymous]
[SecurityHeaders]
public class Callback : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IIdentityServerInteractionService _interaction;
    private readonly ILogger<Callback> _logger;
    private readonly IEventService _events;

    public Callback(
        IIdentityServerInteractionService interaction,
        IEventService events,
        ILogger<Callback> logger,
        UserManager<User> usermanager, SignInManager<User> signInManager)
    {
        _userManager = usermanager;
        _signInManager = signInManager;
        _interaction = interaction;
        _logger = logger;
        _events = events;
    }

    public async Task<IActionResult> OnGet()
    {
        var result = await AuthenticateExternalScheme();

        var (user, provider, providerUserId, claims) = await FindUserFromExternalProviderAsync(result);
        if (user == null)
        {
            user = await AutoProvisionUserAsync(provider, providerUserId, claims);
        }

        var additionalLocalClaims = new List<Claim>();
        var localSignInProps = new AuthenticationProperties();
        ProcessLoginCallbackForOidc(result, additionalLocalClaims, localSignInProps);

        var principal = await _signInManager.CreateUserPrincipalAsync(user);
        additionalLocalClaims.AddRange(principal.Claims);
        var name = principal.FindFirst(JwtClaimTypes.Name)?.Value ?? user.Id;

        await LocalSignIn(user, provider, additionalLocalClaims, localSignInProps, name);

        var returnUrl = result.Properties.Items["returnUrl"] ?? "~/";

        var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
        await _events.RaiseAsync(new UserLoginSuccessEvent(provider, providerUserId,
            user.Id, name, true, context?.Client.ClientId));

        if (context != null)
        {
            if (context.IsNativeClient())
            {
                return this.LoadingPage(returnUrl);
            }
        }

        return Redirect(returnUrl);
    }

    private async Task<AuthenticateResult> AuthenticateExternalScheme()
    {
        var result = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);
        if (result?.Succeeded != true)
        {
            throw new Exception("External authentication error");
        }

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            var externalClaims = result.Principal.Claims.Select(c => $"{c.Type}: {c.Value}");
            _logger.LogDebug("External claims: {@claims}", externalClaims);
        }

        return result;
    }

    private async Task LocalSignIn(User user, string provider, List<Claim> additionalLocalClaims, AuthenticationProperties localSignInProps, string name)
    {
        var isuser = new IdentityServerUser(user.Id)
        {
            DisplayName = name,
            IdentityProvider = provider,
            AdditionalClaims = additionalLocalClaims
        };

        await HttpContext.SignInAsync(isuser, localSignInProps);

        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
    }


    private async Task<(User user, string provider, string providerUserId, IEnumerable<Claim> claims)>
        FindUserFromExternalProviderAsync(AuthenticateResult result)
    {
        var externalUser = result.Principal;

        var userIdClaim = externalUser.FindFirst(JwtClaimTypes.Subject) ??
                          externalUser.FindFirst(ClaimTypes.NameIdentifier) ??
                          throw new Exception("Unknown userid");

        var claims = externalUser.Claims.ToList();
        claims.Remove(userIdClaim);

        var provider = result.Properties.Items["scheme"];
        var providerUserId = userIdClaim.Value;

        var user = await _userManager.FindByLoginAsync(provider, providerUserId);

        return (user, provider, providerUserId, claims);
    }

    private async Task<User> AutoProvisionUserAsync(string provider, string providerUserId, IEnumerable<Claim> claims)
    {
        var filtered = new List<Claim>();

        AddNameToFilteredClaims(claims, filtered);

        string email = AddEmailToFilteredClaims(claims, filtered);

        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            var result = await _userManager.AddLoginAsync(existingUser, new UserLoginInfo(provider, providerUserId, provider));
            if (!result.Succeeded) throw new Exception(result.Errors.First().Description);

            return existingUser;
        }
        else
        {
            return await CreateNewUserWithClaimsAndExternalLogin(provider, providerUserId, filtered, email);
        }
    }

    private async Task<User> CreateNewUserWithClaimsAndExternalLogin(string provider, string providerUserId, List<Claim> filtered, string email)
    {
        var user = new User
        {
            UserName = email,
            Email = email
        };

        var identityResult = await _userManager.CreateAsync(user);
        if (!identityResult.Succeeded) throw new Exception(identityResult.Errors.First().Description);

        if (filtered.Any())
        {
            identityResult = await _userManager.AddClaimsAsync(user, filtered);
            if (!identityResult.Succeeded) throw new Exception(identityResult.Errors.First().Description);
        }

        identityResult = await _userManager.AddLoginAsync(user, new UserLoginInfo(provider, providerUserId, provider));
        if (!identityResult.Succeeded) throw new Exception(identityResult.Errors.First().Description);

        return user;
    }

    private static string AddEmailToFilteredClaims(IEnumerable<Claim> claims, List<Claim> filtered)
    {
        // email
        var email = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Email)?.Value ??
           claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
        if (email != null)
        {
            filtered.Add(new Claim(JwtClaimTypes.Email, email));
        }

        return email;
    }

    private static void AddNameToFilteredClaims(IEnumerable<Claim> claims, List<Claim> filtered)
    {
        var name = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Name)?.Value ??
                        claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
        if (name != null)
        {
            filtered.Add(new Claim(JwtClaimTypes.Name, name));
        }
        else
        {
            var first = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.GivenName)?.Value ??
            claims.FirstOrDefault(x => x.Type == ClaimTypes.GivenName)?.Value;
            var last = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.FamilyName)?.Value ??
                claims.FirstOrDefault(x => x.Type == ClaimTypes.Surname)?.Value;
            if (first != null && last != null)
            {
                filtered.Add(new Claim(JwtClaimTypes.Name, first + " " + last));
            }
            else if (first != null)
            {
                filtered.Add(new Claim(JwtClaimTypes.Name, first));
            }
            else if (last != null)
            {
                filtered.Add(new Claim(JwtClaimTypes.Name, last));
            }
        }
    }

    private void ProcessLoginCallbackForOidc(AuthenticateResult externalResult, List<Claim> localClaims, AuthenticationProperties localSignInProps)
    {
        var sid = externalResult.Principal.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.SessionId);
        if (sid != null)
        {
            localClaims.Add(new Claim(JwtClaimTypes.SessionId, sid.Value));
        }

        var id_token = externalResult.Properties.GetTokenValue("id_token");
        if (id_token != null)
        {
            localSignInProps.StoreTokens(new[] { new AuthenticationToken { Name = "id_token", Value = id_token } });
        }
    }
}
using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace CompanyEmployees.IDP;

public static class Config
{
    public static IEnumerable<IdentityResource> Ids =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Address(),
            new IdentityResource("roles", "User role(s)", new List<string> { "role" }),
            new IdentityResource("country", "Your country", new List<string> { "country" })
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope("companyemployeeapi.scope", "CompanyEmployee API Scope")
        };

    public static IEnumerable<ApiResource> Apis =>
        new ApiResource[]
        {
            new ApiResource("companyemployeeapi", "CompanyEmployee API")
            {
                    Scopes = { "companyemployeeapi.scope" },
                    UserClaims = new List<string> { "role" }
            }
        };

    public static IEnumerable<Client> Clients =>
        new Client[]
        {
            new Client
            {
                ClientName = "CompanyEmployeeClient",
                ClientId = "companyemployeeclient",
                AllowedGrantTypes = GrantTypes.Code,
                RedirectUris = new List<string>{ "https://localhost:5010/signin-oidc" },
                AllowedScopes = 
                { 
                    IdentityServerConstants.StandardScopes.OpenId, 
                    IdentityServerConstants.StandardScopes.Profile, 
                    IdentityServerConstants.StandardScopes.Address,
                    "roles",
                    "companyemployeeapi.scope",
                    "country"
                },
                ClientSecrets = { new Secret("CompanyEmployeeClientSecret".Sha512()) },
                RequirePkce = true,
                RequireConsent = true,
                PostLogoutRedirectUris = new List<string> { "https://localhost:5010/signout-callback-oidc" },
                ClientUri = "https://localhost:5010",
                AccessTokenLifetime = 120,
                AllowOfflineAccess = true,
                UpdateAccessTokenClaimsOnRefresh = true
            }
        };
}
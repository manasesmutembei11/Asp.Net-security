using CompanyEmployees.IDP.Entities;
using CompanyEmployees.IDP.Entities.ViewModels;
using EmailService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CompanyEmployees.IDP.Pages.Account.Login
{
    [AllowAnonymous]
    [BindProperties]
    public class LoginTwoStepModel : BasePage
    {
        private readonly UserManager<User> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly SignInManager<User> _signInManager;

        public TwoStepModel Input { get; set; }
        public string ReturnUrl { get; set; }
        public string Email { get; set; }

        public LoginTwoStepModel(UserManager<User> userManager, IEmailSender emailSender, 
            SignInManager<User> signInManager) : base (userManager, emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> OnGet(string email, bool rememberLogin, string returnUrl)
        {
            ReturnUrl= returnUrl;
            Email = email;
            Input = new TwoStepModel { RememberLogin = rememberLogin };

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return RedirectToPage("/Account/Error", new { ReturnUrl });
            }

            var providers = await _userManager.GetValidTwoFactorProvidersAsync(user);
            if (!providers.Contains("Email"))
            {
                return RedirectToPage("/Account/Error", new { ReturnUrl });
            }

            var token = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");

            var message = new Message(new string[] { email }, "Authentication token", token, null);
            await _emailSender.SendEmailAsync(message);

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return RedirectToPage("/Account/Error", new { ReturnUrl });
            }

            var result = await _signInManager.TwoFactorSignInAsync("Email", Input.TwoFactorCode, Input.RememberLogin, rememberClient: false);
            if (result.Succeeded)
            {
                return this.LoadingPage(ReturnUrl);
            }
            else if (result.IsLockedOut)
            {
                await HandleLockout(Email, ReturnUrl);

                return Page();
            }
            else
            {
                return RedirectToPage("/Account/Error", new { ReturnUrl });
            }
        }
    }
}

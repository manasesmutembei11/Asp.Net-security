using AutoMapper;
using CompanyEmployees.IDP.Entities;
using CompanyEmployees.IDP.Entities.ViewModels;
using EmailService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CompanyEmployees.IDP.Pages.Account.ForgotPassword
{
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly IEmailSender _emailSender;

        [BindProperty]
        public Entities.ViewModels.ForgotPasswordModel Input { get; set; }
        [BindProperty]
        public string ReturnUrl { get; set; }

        public ForgotPasswordModel(UserManager<User> userManager, IEmailSender sender)
        {
            _userManager = userManager;
            _emailSender = sender;
        }

        public IActionResult OnGet(string returnUrl)
        {
            ReturnUrl = returnUrl;

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
                return RedirectToPage("/Account/ForgotPassword/ForgotPasswordConfirmation");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callback = Url.Page("/Account/ResetPassword/ResetPassword", null, new { token, email = user.Email, ReturnUrl }, Request.Scheme);

            var message = new Message(new string[] { user.Email }, "Reset password token", callback, null);
            await _emailSender.SendEmailAsync(message);

            return RedirectToPage("/Account/ForgotPassword/ForgotPasswordConfirmation");

        }
    }
}

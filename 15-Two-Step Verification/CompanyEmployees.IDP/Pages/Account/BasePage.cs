using CompanyEmployees.IDP.Entities;
using EmailService;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CompanyEmployees.IDP.Pages.Account
{
    public class BasePage : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly IEmailSender _emailSender;

        public BasePage(UserManager<User> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        protected async Task HandleLockout(string email, string returnUrl)
        {
            var user = await _userManager.FindByEmailAsync(email);
            var forgotPassLink = Url.Page("/Account/ForgotPassword/ForgotPassword",
                null, new { returnUrl }, Request.Scheme);

            var content = string.Format(@"Your account is locked out, 
                to reset your password, please click this link: {0}", forgotPassLink);

            var message = new Message(new string[] { user.Email },
                "Locked out account information", content, null);
            await _emailSender.SendEmailAsync(message);

            ModelState.AddModelError("", "The account is locked out");
        }
    }
}

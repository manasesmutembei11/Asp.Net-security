using CompanyEmployees.IDP.Entities;
using CompanyEmployees.IDP.Entities.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CompanyEmployees.IDP.Pages.Account.ResetPassword
{
    [AllowAnonymous]
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<User> _userManager;

        [BindProperty]
        public Entities.ViewModels.ResetPasswordModel Input { get; set; }
        [BindProperty]
        public string ReturnUrl { get; set; }

        public ResetPasswordModel(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public IActionResult OnGet(string token, string email, string returnUrl)
        {
            Input = new Entities.ViewModels.ResetPasswordModel { Token = token, Email = email };
            ReturnUrl= returnUrl;

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
                RedirectToPage("/Account/ResetPassword/ResetPasswordConfirmation", new { ReturnUrl });

            var resetPassResult = await _userManager.ResetPasswordAsync(user, Input.Token, Input.Password);
            if (!resetPassResult.Succeeded)
            {
                foreach (var error in resetPassResult.Errors)
                {
                    ModelState.TryAddModelError(error.Code, error.Description);
                }

                return Page();
            }

            return RedirectToPage("/Account/ResetPassword/ResetPasswordConfirmation", new { ReturnUrl }); ;
        }
    }
}

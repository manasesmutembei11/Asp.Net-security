using CompanyEmployees.IDP.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CompanyEmployees.IDP.Pages.Account.Register
{
    [AllowAnonymous]
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<User> _userManager;

        public string ReturnUrl { get; set; }

        public ConfirmEmailModel(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> OnGet(string token, string email, string returnUrl)
        {
            ReturnUrl = returnUrl;

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return RedirectToPage("/Account/Error", new { returnUrl });

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
                return Page();
            else
                return RedirectToPage("/Account/Error", new { returnUrl });
        }
    }
}

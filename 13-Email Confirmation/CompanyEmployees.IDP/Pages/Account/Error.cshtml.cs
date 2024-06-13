using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CompanyEmployees.IDP.Pages.Account
{
    [AllowAnonymous]
    public class ErrorModel : PageModel
    {
        public string ReturnUrl { get; set; }

        public IActionResult OnGet(string returnUrl)
        {
            ReturnUrl = returnUrl;

            return Page();
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CompanyEmployees.IDP.Pages.Account.Register
{
    [AllowAnonymous]
    public class SuccessRegistrationModel : PageModel
    {
        public IActionResult OnGet()
        {
            return Page();
        }
    }
}

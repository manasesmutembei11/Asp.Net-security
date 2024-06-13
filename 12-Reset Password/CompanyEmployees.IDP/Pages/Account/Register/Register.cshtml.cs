using AutoMapper;
using CompanyEmployees.IDP.Entities;
using CompanyEmployees.IDP.Entities.ViewModels;
using IdentityModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace CompanyEmployees.IDP.Pages.Account.Register
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;

        [BindProperty]
        public UserRegistrationModel Input { get; set; }
        [BindProperty]
        public string ReturnUrl { get; set; }

        public RegisterModel(UserManager<User> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public IActionResult OnGet(string returnUrl)
        {
            ReturnUrl = returnUrl;

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = _mapper.Map<User>(Input);

            var result = await _userManager.CreateAsync(user, Input.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.TryAddModelError(error.Code, error.Description);
                }

                return Page();
            }

            await _userManager.AddToRoleAsync(user, "Visitor");

            await _userManager.AddClaimsAsync(user, new List<Claim>
            {
                new Claim(JwtClaimTypes.GivenName, user.FirstName),
                new Claim(JwtClaimTypes.FamilyName, user.LastName),
                new Claim(JwtClaimTypes.Role, "Visitor"),
                new Claim(JwtClaimTypes.Address, user.Address),
                new Claim("country", user.Country)
                });

            return Redirect(ReturnUrl);
        }
    }
}

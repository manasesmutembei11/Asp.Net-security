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
            new IdentityResource("roles", "User role(s)", new List<string> { "role" })
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        { };

    public static IEnumerable<ApiResource> Apis =>
        new ApiResource[]
        { };

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
                    "roles"
                },
                ClientSecrets = { new Secret("CompanyEmployeeClientSecret".Sha512()) },
                RequirePkce = true,
                RequireConsent = true,
                PostLogoutRedirectUris = new List<string> { "https://localhost:5010/signout-callback-oidc" },
                ClientUri = "https://localhost:5010"
            }
        };
}
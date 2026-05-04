using Duende.IdentityServer.Models;

namespace Bazaar.Identity;

public static class IdentityConfig
{
    public static IEnumerable<IdentityResource> IdentityResources =>
    [
        new IdentityResources.OpenId(),
        new IdentityResources.Profile(),
        new IdentityResources.Phone()
    ];

    public static IEnumerable<ApiScope> ApiScopes =>
    [
        new ApiScope("bazaar_api", "Bazaar API")
    ];

    public static IEnumerable<ApiResource> ApiResources =>
    [
        new ApiResource("bazaar_api", "Bazaar API")
        {
            Scopes = { "bazaar_api" }
        }
    ];

    public static IEnumerable<Client> Clients =>
    [
        new Client
        {
            ClientId = "bazaar_mobile",
            ClientName = "Bazaar Mobile App",
            AllowedGrantTypes = { "otp_verification" },
            AllowedScopes = { "openid", "profile", "phone", "bazaar_api" },
            AccessTokenLifetime = 3600 * 24,
            RefreshTokenUsage = TokenUsage.ReUse,
            AllowOfflineAccess = true,
            RequireClientSecret = false
        }
    ];
}

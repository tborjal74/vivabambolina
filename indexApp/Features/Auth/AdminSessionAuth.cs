using System.Security.Claims;
using indexApp.Features.Auth.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace indexApp.Features.Auth;

public static class AdminSessionAuth
{
    public const string Scheme = "AdminSession";
    public const string ExternalScheme = "ExternalOAuth";

    public static ClaimsPrincipal CreatePrincipal(string userId, string email, string role)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, email),
            new Claim(ClaimTypes.Role, role)
        };

        return new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme));
    }

    public static ClaimsPrincipal CreatePrincipal(AdminUser user)
    {
        return CreatePrincipal(user.Id.ToString(), user.Email, user.Role);
    }

    public static AuthenticationProperties CreateProperties(bool rememberDevice)
    {
        return new AuthenticationProperties
        {
            IsPersistent = rememberDevice,
            ExpiresUtc = rememberDevice ? DateTimeOffset.UtcNow.AddDays(14) : null
        };
    }

    public static void ConfigureCookie(CookieAuthenticationOptions options)
    {
        options.Cookie.Name = "vb_admin_session";
        options.LoginPath = "/admin/login";
        options.LogoutPath = "/api/auth/logout";
        options.AccessDeniedPath = "/admin/login";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    }
}

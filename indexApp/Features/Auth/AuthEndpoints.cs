using indexApp.Features.Auth.Models;
using indexApp.Features.AiVisualizer.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace indexApp.Features.Auth;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/auth");

        group.MapPost("/login", async (
            [FromBody] LoginRequest request,
            AuthService authService,
            VisualizerService visualizerService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await authService.LoginAsync(request.Email, request.Password, cancellationToken);
            if (result is null)
            {
                return Results.Unauthorized();
            }

            await visualizerService.DeleteUserSessionMediaAsync(result.UserId, cancellationToken);

            await httpContext.SignInAsync(
                AdminSessionAuth.Scheme,
                AdminSessionAuth.CreatePrincipal(result.UserId, result.Email, result.Role),
                AdminSessionAuth.CreateProperties(request.RememberDevice));

            return Results.Ok(result);
        });

        group.MapPost("/session-login", async (
            [FromForm] LoginRequest request,
            [FromQuery] string? returnUrl,
            AuthService authService,
            VisualizerService visualizerService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await authService.LoginAsync(request.Email, request.Password, cancellationToken);
            if (result is null)
            {
                return Results.Redirect("/admin/login?error=invalid");
            }

            await visualizerService.DeleteUserSessionMediaAsync(result.UserId, cancellationToken);

            await httpContext.SignInAsync(
                AdminSessionAuth.Scheme,
                AdminSessionAuth.CreatePrincipal(result.UserId, result.Email, result.Role),
                AdminSessionAuth.CreateProperties(request.RememberDevice));

            return Results.Redirect(GetSafeReturnUrl(returnUrl));
        });

        group.MapGet("/me", (HttpContext httpContext) =>
        {
            var user = httpContext.User;
            return Results.Ok(new
            {
                Email = user.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value,
                Role = user.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
            });
        }).RequireAuthorization();

        group.MapGet("/oauth/challenge", (IOptions<AuthOptions> options) =>
        {
            if (!options.Value.OAuth.IsConfigured)
            {
                return Results.Problem(
                    title: "OAuth is not configured.",
                    detail: "Set Auth:OAuth client credentials and endpoints before using OAuth sign-in.",
                    statusCode: StatusCodes.Status501NotImplemented);
            }

            return Results.Challenge(
                new AuthenticationProperties { RedirectUri = "/api/auth/oauth/callback" },
                ["OAuth"]);
        });

        group.MapGet("/oauth/callback", async (
            HttpContext httpContext,
            AuthService authService,
            VisualizerService visualizerService,
            CancellationToken cancellationToken) =>
        {
            var result = await httpContext.AuthenticateAsync(AdminSessionAuth.ExternalScheme);
            if (!result.Succeeded)
            {
                return Results.BadRequest("OAuth sign-in did not complete.");
            }

            var providerUserId = result.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = result.Principal?.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrWhiteSpace(providerUserId))
            {
                await httpContext.SignOutAsync(AdminSessionAuth.ExternalScheme);
                return Results.BadRequest("OAuth provider did not return a stable user identifier.");
            }

            var adminUser = await authService.ValidateOAuthUserAsync(
                "OAuth",
                providerUserId,
                email,
                cancellationToken);

            await httpContext.SignOutAsync(AdminSessionAuth.ExternalScheme);

            if (adminUser is null)
            {
                return Results.Redirect("/admin/login?error=oauth");
            }

            await visualizerService.DeleteUserSessionMediaAsync(adminUser.Id.ToString(), cancellationToken);

            await httpContext.SignInAsync(
                AdminSessionAuth.Scheme,
                AdminSessionAuth.CreatePrincipal(adminUser),
                AdminSessionAuth.CreateProperties(rememberDevice: true));

            return Results.Redirect("/");
        });

        group.MapPost("/logout", async (
            HttpContext httpContext,
            IAntiforgery antiforgery,
            VisualizerService visualizerService,
            CancellationToken cancellationToken) =>
        {
            if (!await antiforgery.IsRequestValidAsync(httpContext))
            {
                return Results.BadRequest("Invalid logout request.");
            }

            var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrWhiteSpace(userId))
            {
                await visualizerService.DeleteUserSessionMediaAsync(userId, cancellationToken);
            }

            await httpContext.SignOutAsync(AdminSessionAuth.Scheme);
            await httpContext.SignOutAsync(AdminSessionAuth.ExternalScheme);
            return Results.Redirect("/admin/login?loggedOut=1");
        }).RequireAuthorization();

        return endpoints;
    }

    private static string GetSafeReturnUrl(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            return "/";
        }

        return Uri.TryCreate(returnUrl, UriKind.Relative, out _)
            && !returnUrl.StartsWith("//", StringComparison.Ordinal)
            ? returnUrl
            : "/";
    }
}

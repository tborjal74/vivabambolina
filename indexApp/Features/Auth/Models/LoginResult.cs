namespace indexApp.Features.Auth.Models;

public sealed record LoginResult(
    string UserId,
    string AccessToken,
    DateTimeOffset ExpiresAtUtc,
    string Email,
    string Role);

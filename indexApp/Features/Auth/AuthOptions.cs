namespace indexApp.Features.Auth;

public sealed class AuthOptions
{
    public const string SectionName = "Auth";

    public JwtOptions Jwt { get; set; } = new();

    public SeedAdminOptions SeedAdmin { get; set; } = new();

    public OAuthOptions OAuth { get; set; } = new();
}

public sealed class JwtOptions
{
    public string Issuer { get; set; } = "VivaBambolina";

    public string Audience { get; set; } = "VivaBambolina.Admin";

    public string SigningKey { get; set; } = string.Empty;

    public int ExpirationMinutes { get; set; } = 60;
}

public sealed class SeedAdminOptions
{
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}

public sealed class OAuthOptions
{
    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    public string AuthorizationEndpoint { get; set; } = string.Empty;

    public string TokenEndpoint { get; set; } = string.Empty;

    public string UserInformationEndpoint { get; set; } = string.Empty;

    public string CallbackPath { get; set; } = "/signin-oauth";

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(ClientId)
        && !string.IsNullOrWhiteSpace(ClientSecret)
        && !string.IsNullOrWhiteSpace(AuthorizationEndpoint)
        && !string.IsNullOrWhiteSpace(TokenEndpoint);
}

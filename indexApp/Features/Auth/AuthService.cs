using indexApp.Features.Auth.Data;
using indexApp.Features.Auth.Models;
using Microsoft.EntityFrameworkCore;

namespace indexApp.Features.Auth;

public sealed class AuthService
{
    private readonly AuthDbContext dbContext;
    private readonly IJwtTokenService jwtTokenService;
    private readonly IPasswordHasher passwordHasher;

    public AuthService(
        AuthDbContext dbContext,
        IJwtTokenService jwtTokenService,
        IPasswordHasher passwordHasher)
    {
        this.dbContext = dbContext;
        this.jwtTokenService = jwtTokenService;
        this.passwordHasher = passwordHasher;
    }

    public async Task<LoginResult?> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToUpperInvariant();
        var user = await dbContext.AdminUsers
            .SingleOrDefaultAsync(candidate => candidate.NormalizedEmail == normalizedEmail, cancellationToken);

        if (user is null || !user.IsActive || !passwordHasher.Verify(password, user.PasswordHash))
        {
            return null;
        }

        user.LastLoginAtUtc = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return jwtTokenService.CreateToken(user);
    }

    public async Task<AdminUser?> ValidateOAuthUserAsync(
        string provider,
        string providerUserId,
        string? email,
        CancellationToken cancellationToken = default)
    {
        var externalLogin = await dbContext.AdminUserExternalLogins
            .Include(login => login.AdminUser)
            .SingleOrDefaultAsync(
                login => login.Provider == provider && login.ProviderUserId == providerUserId,
                cancellationToken);

        if (externalLogin?.AdminUser is { IsActive: true } linkedUser)
        {
            linkedUser.LastLoginAtUtc = DateTimeOffset.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
            return linkedUser;
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        var normalizedEmail = email.Trim().ToUpperInvariant();
        var adminUser = await dbContext.AdminUsers
            .SingleOrDefaultAsync(user => user.NormalizedEmail == normalizedEmail, cancellationToken);

        if (adminUser is null || !adminUser.IsActive)
        {
            return null;
        }

        dbContext.AdminUserExternalLogins.Add(new AdminUserExternalLogin
        {
            AdminUserId = adminUser.Id,
            Provider = provider,
            ProviderUserId = providerUserId,
            ProviderEmail = email.Trim()
        });

        adminUser.LastLoginAtUtc = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return adminUser;
    }
}

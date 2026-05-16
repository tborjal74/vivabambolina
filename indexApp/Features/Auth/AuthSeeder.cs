using indexApp.Features.Auth.Data;
using indexApp.Features.Auth.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace indexApp.Features.Auth;

public sealed class AuthSeeder
{
    private readonly AuthDbContext dbContext;
    private readonly IPasswordHasher passwordHasher;
    private readonly IOptions<AuthOptions> options;

    public AuthSeeder(AuthDbContext dbContext, IPasswordHasher passwordHasher, IOptions<AuthOptions> options)
    {
        this.dbContext = dbContext;
        this.passwordHasher = passwordHasher;
        this.options = options;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var seedAdmin = options.Value.SeedAdmin;
        if (string.IsNullOrWhiteSpace(seedAdmin.Email) || string.IsNullOrWhiteSpace(seedAdmin.Password))
        {
            return;
        }

        var normalizedEmail = seedAdmin.Email.Trim().ToUpperInvariant();
        var exists = await dbContext.AdminUsers
            .AnyAsync(user => user.NormalizedEmail == normalizedEmail, cancellationToken);

        if (exists)
        {
            return;
        }

        dbContext.AdminUsers.Add(new AdminUser
        {
            Email = seedAdmin.Email.Trim(),
            NormalizedEmail = normalizedEmail,
            PasswordHash = passwordHasher.Hash(seedAdmin.Password),
            Role = "Admin",
            IsActive = true
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

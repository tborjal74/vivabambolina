using indexApp.Features.Auth.Models;
using Microsoft.EntityFrameworkCore;

namespace indexApp.Features.Auth.Data;

public sealed class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options)
        : base(options)
    {
    }

    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();

    public DbSet<AdminUserExternalLogin> AdminUserExternalLogins => Set<AdminUserExternalLogin>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AdminUser>(entity =>
        {
            entity.ToTable("admin_users");
            entity.HasKey(user => user.Id);
            entity.HasIndex(user => user.NormalizedEmail).IsUnique();

            entity.Property(user => user.Id).HasColumnName("id");
            entity.Property(user => user.Email).HasMaxLength(320).IsRequired();
            entity.Property(user => user.Email).HasColumnName("email");
            entity.Property(user => user.NormalizedEmail).HasColumnName("normalized_email").HasMaxLength(320).IsRequired();
            entity.Property(user => user.PasswordHash).HasColumnName("password_hash").HasMaxLength(200).IsRequired();
            entity.Property(user => user.Role).HasColumnName("role").HasMaxLength(64).IsRequired();
            entity.Property(user => user.IsActive).HasColumnName("is_active");
            entity.Property(user => user.CreatedAtUtc).HasColumnName("created_at_utc");
            entity.Property(user => user.LastLoginAtUtc).HasColumnName("last_login_at_utc");
        });

        modelBuilder.Entity<AdminUserExternalLogin>(entity =>
        {
            entity.ToTable("admin_user_external_logins");
            entity.HasKey(login => login.Id);
            entity.HasIndex(login => new { login.Provider, login.ProviderUserId }).IsUnique();

            entity.Property(login => login.Id).HasColumnName("id");
            entity.Property(login => login.AdminUserId).HasColumnName("admin_user_id");
            entity.Property(login => login.Provider).HasColumnName("provider").HasMaxLength(100).IsRequired();
            entity.Property(login => login.ProviderUserId).HasColumnName("provider_user_id").HasMaxLength(200).IsRequired();
            entity.Property(login => login.ProviderEmail).HasColumnName("provider_email").HasMaxLength(320);
            entity.Property(login => login.CreatedAtUtc).HasColumnName("created_at_utc");

            entity.HasOne(login => login.AdminUser)
                .WithMany(user => user.ExternalLogins)
                .HasForeignKey(login => login.AdminUserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

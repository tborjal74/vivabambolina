using System;
using indexApp.Features.Auth.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace indexApp.Features.Auth.Data.Migrations;

[DbContext(typeof(AuthDbContext))]
partial class AuthDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "10.0.8");

        modelBuilder.Entity("indexApp.Features.Auth.Models.AdminUser", entity =>
        {
            entity.Property<Guid>("Id").HasColumnName("id");
            entity.Property<DateTimeOffset>("CreatedAtUtc").HasColumnName("created_at_utc");
            entity.Property<string>("Email").IsRequired().HasMaxLength(320).HasColumnName("email");
            entity.Property<bool>("IsActive").HasColumnName("is_active");
            entity.Property<DateTimeOffset?>("LastLoginAtUtc").HasColumnName("last_login_at_utc");
            entity.Property<string>("NormalizedEmail").IsRequired().HasMaxLength(320).HasColumnName("normalized_email");
            entity.Property<string>("PasswordHash").IsRequired().HasMaxLength(200).HasColumnName("password_hash");
            entity.Property<string>("Role").IsRequired().HasMaxLength(64).HasColumnName("role");

            entity.HasKey("Id");
            entity.HasIndex("NormalizedEmail").IsUnique();
            entity.ToTable("admin_users");
        });

        modelBuilder.Entity("indexApp.Features.Auth.Models.AdminUserExternalLogin", entity =>
        {
            entity.Property<Guid>("Id").HasColumnName("id");
            entity.Property<Guid>("AdminUserId").HasColumnName("admin_user_id");
            entity.Property<DateTimeOffset>("CreatedAtUtc").HasColumnName("created_at_utc");
            entity.Property<string>("Provider").IsRequired().HasMaxLength(100).HasColumnName("provider");
            entity.Property<string>("ProviderEmail").HasMaxLength(320).HasColumnName("provider_email");
            entity.Property<string>("ProviderUserId").IsRequired().HasMaxLength(200).HasColumnName("provider_user_id");

            entity.HasKey("Id");
            entity.HasIndex("AdminUserId");
            entity.HasIndex("Provider", "ProviderUserId").IsUnique();
            entity.ToTable("admin_user_external_logins");

            entity.HasOne("indexApp.Features.Auth.Models.AdminUser", "AdminUser")
                .WithMany("ExternalLogins")
                .HasForeignKey("AdminUserId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            entity.Navigation("AdminUser");
        });

        modelBuilder.Entity("indexApp.Features.Auth.Models.AdminUser", entity =>
        {
            entity.Navigation("ExternalLogins");
        });
    }
}

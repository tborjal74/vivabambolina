using System;
using indexApp.Features.Auth.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace indexApp.Features.Auth.Data.Migrations;

[DbContext(typeof(AuthDbContext))]
[Migration("20260516150000_InitialAuth")]
public partial class InitialAuth : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "admin_users",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                normalized_email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                password_hash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                role = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false),
                created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                last_login_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_admin_users", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "admin_user_external_logins",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                admin_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                provider = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                provider_user_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                provider_email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_admin_user_external_logins", x => x.id);
                table.ForeignKey(
                    name: "fk_admin_user_external_logins_admin_users_admin_user_id",
                    column: x => x.admin_user_id,
                    principalTable: "admin_users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_admin_user_external_logins_admin_user_id",
            table: "admin_user_external_logins",
            column: "admin_user_id");

        migrationBuilder.CreateIndex(
            name: "ix_admin_user_external_logins_provider_provider_user_id",
            table: "admin_user_external_logins",
            columns: ["provider", "provider_user_id"],
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_admin_users_normalized_email",
            table: "admin_users",
            column: "normalized_email",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "admin_user_external_logins");
        migrationBuilder.DropTable(name: "admin_users");
    }
}

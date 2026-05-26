using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace indexApp.Features.AiVisualizer.Data.Migrations
{
    /// <inheritdoc />
    public partial class GeneratedPreviewViews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "view",
                table: "generated_previews",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "Front");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "view",
                table: "generated_previews");
        }
    }
}

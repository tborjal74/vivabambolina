using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace indexApp.Features.AiVisualizer.Data.Migrations
{
    /// <inheritdoc />
    public partial class ExactPaletteColorSelection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "selected_color_hex_code",
                table: "visualizer_requests",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "selected_color_name",
                table: "visualizer_requests",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "selected_color_hex_code",
                table: "visualizer_requests");

            migrationBuilder.DropColumn(
                name: "selected_color_name",
                table: "visualizer_requests");
        }
    }
}

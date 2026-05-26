using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace indexApp.Features.AiVisualizer.Data.Migrations
{
    /// <inheritdoc />
    public partial class DressBuilderSelections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "accessories",
                table: "visualizer_requests",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "back_closure",
                table: "visualizer_requests",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "bodice_design",
                table: "visualizer_requests",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "dress_template",
                table: "visualizer_requests",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "fabric_pattern",
                table: "visualizer_requests",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "fabric_type",
                table: "visualizer_requests",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "waist_shape",
                table: "visualizer_requests",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "accessories",
                table: "visualizer_requests");

            migrationBuilder.DropColumn(
                name: "back_closure",
                table: "visualizer_requests");

            migrationBuilder.DropColumn(
                name: "bodice_design",
                table: "visualizer_requests");

            migrationBuilder.DropColumn(
                name: "dress_template",
                table: "visualizer_requests");

            migrationBuilder.DropColumn(
                name: "fabric_pattern",
                table: "visualizer_requests");

            migrationBuilder.DropColumn(
                name: "fabric_type",
                table: "visualizer_requests");

            migrationBuilder.DropColumn(
                name: "waist_shape",
                table: "visualizer_requests");
        }
    }
}

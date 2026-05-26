using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace indexApp.Features.AiVisualizer.Data.Migrations
{
    /// <inheritdoc />
    public partial class GownSpecificationMeasurements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "wrist",
                table: "customer_measurements",
                newName: "end_of_sleeve");

            migrationBuilder.RenameColumn(
                name: "shoulder_width",
                table: "customer_measurements",
                newName: "shoulder_to_shoulder");

            migrationBuilder.RenameColumn(
                name: "dress_length",
                table: "customer_measurements",
                newName: "shoulder_to_hem");

            migrationBuilder.RenameColumn(
                name: "bust",
                table: "customer_measurements",
                newName: "chest");

            migrationBuilder.DropColumn(
                name: "height",
                table: "customer_measurements");

            migrationBuilder.DropColumn(
                name: "hips",
                table: "customer_measurements");

            migrationBuilder.AddColumn<decimal>(
                name: "bicep",
                table: "customer_measurements",
                type: "numeric(6,2)",
                precision: 6,
                scale: 2,
                nullable: false,
                defaultValue: 28m);

            migrationBuilder.AddColumn<decimal>(
                name: "center_neck_to_waist_point",
                table: "customer_measurements",
                type: "numeric(6,2)",
                precision: 6,
                scale: 2,
                nullable: false,
                defaultValue: 42m);

            migrationBuilder.AddColumn<decimal>(
                name: "cuff_height",
                table: "customer_measurements",
                type: "numeric(6,2)",
                precision: 6,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "has_built_in_puffy",
                table: "customer_measurements",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "has_horsehair_edge_hem",
                table: "customer_measurements",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "mock_neck_circumference",
                table: "customer_measurements",
                type: "numeric(6,2)",
                precision: 6,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "mock_neck_height",
                table: "customer_measurements",
                type: "numeric(6,2)",
                precision: 6,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "neckline_shape",
                table: "customer_measurements",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "Sweetheart");

            migrationBuilder.AddColumn<decimal>(
                name: "shoulder_to_neck_opening",
                table: "customer_measurements",
                type: "numeric(6,2)",
                precision: 6,
                scale: 2,
                nullable: false,
                defaultValue: 18m);

            migrationBuilder.AddColumn<decimal>(
                name: "shoulder_to_side_neck",
                table: "customer_measurements",
                type: "numeric(6,2)",
                precision: 6,
                scale: 2,
                nullable: false,
                defaultValue: 13m);

            migrationBuilder.AddColumn<decimal>(
                name: "shoulder_to_waist_seam",
                table: "customer_measurements",
                type: "numeric(6,2)",
                precision: 6,
                scale: 2,
                nullable: false,
                defaultValue: 40m);

            migrationBuilder.AddColumn<string>(
                name: "size",
                table: "customer_measurements",
                type: "character varying(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "M");

            migrationBuilder.AddColumn<string>(
                name: "skirt_shape",
                table: "customer_measurements",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "A-Line");

            migrationBuilder.AddColumn<string>(
                name: "sleeve_shape",
                table: "customer_measurements",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "Sleeveless");

            migrationBuilder.AddColumn<decimal>(
                name: "underarm_to_side_waist_seam",
                table: "customer_measurements",
                type: "numeric(6,2)",
                precision: 6,
                scale: 2,
                nullable: false,
                defaultValue: 22m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "bicep",
                table: "customer_measurements");

            migrationBuilder.DropColumn(
                name: "center_neck_to_waist_point",
                table: "customer_measurements");

            migrationBuilder.DropColumn(
                name: "cuff_height",
                table: "customer_measurements");

            migrationBuilder.DropColumn(
                name: "has_built_in_puffy",
                table: "customer_measurements");

            migrationBuilder.DropColumn(
                name: "has_horsehair_edge_hem",
                table: "customer_measurements");

            migrationBuilder.DropColumn(
                name: "mock_neck_circumference",
                table: "customer_measurements");

            migrationBuilder.DropColumn(
                name: "mock_neck_height",
                table: "customer_measurements");

            migrationBuilder.DropColumn(
                name: "neckline_shape",
                table: "customer_measurements");

            migrationBuilder.DropColumn(
                name: "shoulder_to_neck_opening",
                table: "customer_measurements");

            migrationBuilder.DropColumn(
                name: "shoulder_to_side_neck",
                table: "customer_measurements");

            migrationBuilder.DropColumn(
                name: "shoulder_to_waist_seam",
                table: "customer_measurements");

            migrationBuilder.DropColumn(
                name: "size",
                table: "customer_measurements");

            migrationBuilder.DropColumn(
                name: "skirt_shape",
                table: "customer_measurements");

            migrationBuilder.DropColumn(
                name: "sleeve_shape",
                table: "customer_measurements");

            migrationBuilder.DropColumn(
                name: "underarm_to_side_waist_seam",
                table: "customer_measurements");

            migrationBuilder.RenameColumn(
                name: "end_of_sleeve",
                table: "customer_measurements",
                newName: "wrist");

            migrationBuilder.RenameColumn(
                name: "shoulder_to_shoulder",
                table: "customer_measurements",
                newName: "shoulder_width");

            migrationBuilder.RenameColumn(
                name: "shoulder_to_hem",
                table: "customer_measurements",
                newName: "dress_length");

            migrationBuilder.RenameColumn(
                name: "chest",
                table: "customer_measurements",
                newName: "bust");

            migrationBuilder.AddColumn<decimal>(
                name: "height",
                table: "customer_measurements",
                type: "numeric(6,2)",
                precision: 6,
                scale: 2,
                nullable: false,
                defaultValue: 165m);

            migrationBuilder.AddColumn<decimal>(
                name: "hips",
                table: "customer_measurements",
                type: "numeric(6,2)",
                precision: 6,
                scale: 2,
                nullable: false,
                defaultValue: 91m);
        }
    }
}

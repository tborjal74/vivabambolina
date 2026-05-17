using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace indexApp.Features.AiVisualizer.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialVisualizer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ai_usage_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    visualizer_request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    operation = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    estimated_input_tokens = table.Column<int>(type: "integer", nullable: false),
                    estimated_output_tokens = table.Column<int>(type: "integer", nullable: false),
                    cost_estimate = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ai_usage_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "customer_measurements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    height = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    bust = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    waist = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    hips = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    shoulder_width = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    dress_length = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    sleeve_length = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    wrist = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer_measurements", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "dress_styles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    description = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: false),
                    base_price = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    silhouette = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    neckline = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    sleeve_type = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    dress_length = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    template_image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dress_styles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fabrics",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    description = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: false),
                    texture_image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    material_type = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fabrics", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fabric_colors",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fabric_id = table.Column<Guid>(type: "uuid", nullable: false),
                    color_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    hex_code = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    swatch_image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fabric_colors", x => x.id);
                    table.ForeignKey(
                        name: "FK_fabric_colors_fabrics_fabric_id",
                        column: x => x.fabric_id,
                        principalTable: "fabrics",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "visualizer_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    dress_style_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fabric_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fabric_color_id = table.Column<Guid>(type: "uuid", nullable: false),
                    measurement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    uploaded_photo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    basic_preview_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    prompt_used = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    error_message = table.Column<string>(type: "character varying(800)", maxLength: 800, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_visualizer_requests", x => x.id);
                    table.ForeignKey(
                        name: "FK_visualizer_requests_customer_measurements_measurement_id",
                        column: x => x.measurement_id,
                        principalTable: "customer_measurements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_visualizer_requests_dress_styles_dress_style_id",
                        column: x => x.dress_style_id,
                        principalTable: "dress_styles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_visualizer_requests_fabric_colors_fabric_color_id",
                        column: x => x.fabric_color_id,
                        principalTable: "fabric_colors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_visualizer_requests_fabrics_fabric_id",
                        column: x => x.fabric_id,
                        principalTable: "fabrics",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "generated_previews",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    visualizer_request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ai_provider = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    image_size = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    quality = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    generation_cost = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_generated_previews", x => x.id);
                    table.ForeignKey(
                        name: "FK_generated_previews_visualizer_requests_visualizer_request_id",
                        column: x => x.visualizer_request_id,
                        principalTable: "visualizer_requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_fabric_colors_fabric_id",
                table: "fabric_colors",
                column: "fabric_id");

            migrationBuilder.CreateIndex(
                name: "IX_generated_previews_visualizer_request_id",
                table: "generated_previews",
                column: "visualizer_request_id");

            migrationBuilder.CreateIndex(
                name: "IX_visualizer_requests_dress_style_id",
                table: "visualizer_requests",
                column: "dress_style_id");

            migrationBuilder.CreateIndex(
                name: "IX_visualizer_requests_fabric_color_id",
                table: "visualizer_requests",
                column: "fabric_color_id");

            migrationBuilder.CreateIndex(
                name: "IX_visualizer_requests_fabric_id",
                table: "visualizer_requests",
                column: "fabric_id");

            migrationBuilder.CreateIndex(
                name: "IX_visualizer_requests_measurement_id",
                table: "visualizer_requests",
                column: "measurement_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ai_usage_logs");

            migrationBuilder.DropTable(
                name: "generated_previews");

            migrationBuilder.DropTable(
                name: "visualizer_requests");

            migrationBuilder.DropTable(
                name: "customer_measurements");

            migrationBuilder.DropTable(
                name: "dress_styles");

            migrationBuilder.DropTable(
                name: "fabric_colors");

            migrationBuilder.DropTable(
                name: "fabrics");
        }
    }
}

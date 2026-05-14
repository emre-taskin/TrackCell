using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TrackCell.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNcManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "non_conformances",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_non_conformances", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "part_images",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    part_definition_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    content_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    uploaded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_part_images", x => x.id);
                    table.ForeignKey(
                        name: "fk_part_images_part_definitions_part_definition_id",
                        column: x => x.part_definition_id,
                        principalTable: "part_definitions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "image_zones",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    part_image_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    x = table.Column<double>(type: "double precision", nullable: false),
                    y = table.Column<double>(type: "double precision", nullable: false),
                    width = table.Column<double>(type: "double precision", nullable: false),
                    height = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_image_zones", x => x.id);
                    table.ForeignKey(
                        name: "fk_image_zones_part_images_part_image_id",
                        column: x => x.part_image_id,
                        principalTable: "part_images",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "image_zone_non_conformances",
                columns: table => new
                {
                    image_zone_id = table.Column<int>(type: "integer", nullable: false),
                    non_conformance_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_image_zone_non_conformances", x => new { x.image_zone_id, x.non_conformance_id });
                    table.ForeignKey(
                        name: "fk_image_zone_non_conformances_image_zones_image_zone_id",
                        column: x => x.image_zone_id,
                        principalTable: "image_zones",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_image_zone_non_conformances_non_conformances_non_conformance_id",
                        column: x => x.non_conformance_id,
                        principalTable: "non_conformances",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "non_conformances",
                columns: new[] { "id", "code", "description" },
                values: new object[,]
                {
                    { 1, "NC-SCR", "Scratch" },
                    { 2, "NC-DNT", "Dent" },
                    { 3, "NC-MIS", "Missing Fastener" },
                    { 4, "NC-ALN", "Misalignment" },
                    { 5, "NC-COR", "Corrosion" },
                    { 6, "NC-CLR", "Wrong Color" },
                    { 7, "NC-CRK", "Crack" },
                    { 8, "NC-BRR", "Burr / Sharp Edge" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_non_conformances_code",
                table: "non_conformances",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_part_images_part_definition_id",
                table: "part_images",
                column: "part_definition_id");

            migrationBuilder.CreateIndex(
                name: "ix_image_zones_part_image_id",
                table: "image_zones",
                column: "part_image_id");

            migrationBuilder.CreateIndex(
                name: "ix_image_zone_non_conformances_non_conformance_id",
                table: "image_zone_non_conformances",
                column: "non_conformance_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "image_zone_non_conformances");
            migrationBuilder.DropTable(name: "image_zones");
            migrationBuilder.DropTable(name: "part_images");
            migrationBuilder.DropTable(name: "non_conformances");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TrackCell.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInspectionResults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "inspection_results",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    part_image_id = table.Column<int>(type: "integer", nullable: false),
                    image_zone_id = table.Column<int>(type: "integer", nullable: false),
                    non_conformance_id = table.Column<int>(type: "integer", nullable: false),
                    serial_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    inspected_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inspection_results", x => x.id);
                    table.ForeignKey(
                        name: "fk_inspection_results_part_images_part_image_id",
                        column: x => x.part_image_id,
                        principalTable: "part_images",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_inspection_results_image_zones_image_zone_id",
                        column: x => x.image_zone_id,
                        principalTable: "image_zones",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_inspection_results_non_conformances_non_conformance_id",
                        column: x => x.non_conformance_id,
                        principalTable: "non_conformances",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_inspection_results_part_image_id",
                table: "inspection_results",
                column: "part_image_id");

            migrationBuilder.CreateIndex(
                name: "ix_inspection_results_image_zone_id",
                table: "inspection_results",
                column: "image_zone_id");

            migrationBuilder.CreateIndex(
                name: "ix_inspection_results_non_conformance_id",
                table: "inspection_results",
                column: "non_conformance_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "inspection_results");
        }
    }
}

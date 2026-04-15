using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TrackCell.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialSnakeCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "operation_definitions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    op_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_operation_definitions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "operation_histories",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    badge_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    part_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    serial_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    op_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    action_level = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_operation_histories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "operators",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    badge_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_operators", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "part_definitions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    part_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_part_definitions", x => x.id);
                });

            migrationBuilder.InsertData(
                table: "operation_definitions",
                columns: new[] { "id", "description", "op_number" },
                values: new object[,]
                {
                    { 1, "Machining", "OP-10" },
                    { 2, "Sub-Assembly", "OP-20" },
                    { 3, "Primary Assembly", "OP-30" },
                    { 4, "Testing and QA", "OP-40" },
                    { 5, "Packaging", "OP-50" }
                });

            migrationBuilder.InsertData(
                table: "operators",
                columns: new[] { "id", "badge_number", "name" },
                values: new object[,]
                {
                    { 1, "EMP-1001", "Alice Smith" },
                    { 2, "EMP-1002", "Bob Johnson" },
                    { 3, "EMP-1003", "Charlie Brown" },
                    { 4, "EMP-1004", "Diana Prince" }
                });

            migrationBuilder.InsertData(
                table: "part_definitions",
                columns: new[] { "id", "description", "part_number" },
                values: new object[,]
                {
                    { 1, "Chassis Assembly", "PRT-001X" },
                    { 2, "Engine Block", "PRT-002Y" },
                    { 3, "Transmission Unit", "PRT-003Z" },
                    { 4, "Wiring Harness", "PRT-004A" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "operation_definitions");

            migrationBuilder.DropTable(
                name: "operation_histories");

            migrationBuilder.DropTable(
                name: "operators");

            migrationBuilder.DropTable(
                name: "part_definitions");
        }
    }
}

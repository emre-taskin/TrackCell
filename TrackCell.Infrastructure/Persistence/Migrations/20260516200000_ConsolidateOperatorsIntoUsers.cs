using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TrackCell.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ConsolidateOperatorsIntoUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "operators");

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "id", "windows_account", "display_name", "role", "badge_number" },
                values: new object[] { 5, "DOMAIN\\ewatson", "Eve Watson", "Inspector", "EMP-1005" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "id",
                keyValue: 5);

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
        }
    }
}

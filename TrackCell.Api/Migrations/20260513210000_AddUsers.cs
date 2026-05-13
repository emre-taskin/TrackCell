using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TrackCell.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    windows_account = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    display_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    badge_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "id", "windows_account", "display_name", "role", "badge_number" },
                values: new object[,]
                {
                    { 1, "DOMAIN\\asmith",   "Alice Smith",   "Operator",   "EMP-1001" },
                    { 2, "DOMAIN\\bjohnson", "Bob Johnson",   "Operator",   "EMP-1002" },
                    { 3, "DOMAIN\\cbrown",   "Charlie Brown", "Supervisor", "EMP-1003" },
                    { 4, "DOMAIN\\dprince",  "Diana Prince",  "Admin",      "EMP-1004" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_users_windows_account",
                table: "users",
                column: "windows_account",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "users");
        }
    }
}

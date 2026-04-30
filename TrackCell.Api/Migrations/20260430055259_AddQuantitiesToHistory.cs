using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrackCell.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddQuantitiesToHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "good_qty",
                table: "operation_histories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "scrap_code",
                table: "operation_histories",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "scrap_qty",
                table: "operation_histories",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "good_qty",
                table: "operation_histories");

            migrationBuilder.DropColumn(
                name: "scrap_code",
                table: "operation_histories");

            migrationBuilder.DropColumn(
                name: "scrap_qty",
                table: "operation_histories");
        }
    }
}

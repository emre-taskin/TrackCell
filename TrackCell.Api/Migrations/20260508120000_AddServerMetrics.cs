using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TrackCell.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddServerMetrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "server_metrics",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    machine_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    cpu_usage_percent = table.Column<double>(type: "double precision", nullable: false),
                    total_memory_bytes = table.Column<long>(type: "bigint", nullable: false),
                    available_memory_bytes = table.Column<long>(type: "bigint", nullable: false),
                    memory_usage_percent = table.Column<double>(type: "double precision", nullable: false),
                    total_disk_bytes = table.Column<long>(type: "bigint", nullable: false),
                    available_disk_bytes = table.Column<long>(type: "bigint", nullable: false),
                    disk_usage_percent = table.Column<double>(type: "double precision", nullable: false),
                    uptime_seconds = table.Column<long>(type: "bigint", nullable: false),
                    health_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_server_metrics", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_server_metrics_machine_name_timestamp",
                table: "server_metrics",
                columns: new[] { "machine_name", "timestamp" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "server_metrics");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SwiftRoute.API.Migrations
{
    /// <inheritdoc />
    public partial class ExpandMaintenanceLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ServiceType",
                table: "MaintenanceLogs",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "MaintenanceLogs",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Technician",
                table: "MaintenanceLogs",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ServiceType",
                table: "MaintenanceLogs");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "MaintenanceLogs");

            migrationBuilder.DropColumn(
                name: "Technician",
                table: "MaintenanceLogs");
        }
    }
}

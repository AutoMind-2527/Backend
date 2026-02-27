using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoMindBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddTrackerCodeToVehicle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TrackerCode",
                table: "Vehicles",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsClaimed",
                table: "Vehicles",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TrackerCode",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "IsClaimed",
                table: "Vehicles");
        }
    }
}

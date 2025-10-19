using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoMindBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Trips");

            migrationBuilder.AddColumn<string>(
                name: "Rolle",
                table: "Trips",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rolle",
                table: "Trips");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Trips",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}

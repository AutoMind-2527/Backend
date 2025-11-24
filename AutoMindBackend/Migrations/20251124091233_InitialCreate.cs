using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoMindBackend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GpsData");

            migrationBuilder.DropColumn(
                name: "AverageSpeed",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "FuelUsed",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "TripCost",
                table: "Trips");

            migrationBuilder.AlterColumn<int>(
                name: "Mileage",
                table: "Vehicles",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "REAL");

            migrationBuilder.AlterColumn<double>(
                name: "FuelConsumption",
                table: "Vehicles",
                type: "decimal(5,2)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "REAL");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "Users",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "User",
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordSalt",
                table: "Users",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "BLOB");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "BLOB");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Users",
                type: "TEXT",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "KeycloakId",
                table: "Users",
                type: "TEXT",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<double>(
                name: "DistanceKm",
                table: "Trips",
                type: "decimal(10,2)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "REAL");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "KeycloakId", "PasswordHash", "PasswordSalt", "Role", "Username" },
                values: new object[] { 1, "admin@automind.com", "admin-keycloak-id", "", "", "Admin", "admin" });

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_UserId",
                table: "Vehicles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_KeycloakId",
                table: "Users",
                column: "KeycloakId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trips_UserId",
                table: "Trips",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Trips_VehicleId",
                table: "Trips",
                column: "VehicleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Trips_Users_UserId",
                table: "Trips",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Trips_Vehicles_VehicleId",
                table: "Trips",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_Users_UserId",
                table: "Vehicles",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trips_Users_UserId",
                table: "Trips");

            migrationBuilder.DropForeignKey(
                name: "FK_Trips_Vehicles_VehicleId",
                table: "Trips");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_Users_UserId",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_UserId",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Users_KeycloakId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Username",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Trips_UserId",
                table: "Trips");

            migrationBuilder.DropIndex(
                name: "IX_Trips_VehicleId",
                table: "Trips");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "KeycloakId",
                table: "Users");

            migrationBuilder.AlterColumn<double>(
                name: "Mileage",
                table: "Vehicles",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<double>(
                name: "FuelConsumption",
                table: "Vehicles",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "decimal(5,2)");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "Users",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50,
                oldDefaultValue: "User");

            migrationBuilder.AlterColumn<byte[]>(
                name: "PasswordSalt",
                table: "Users",
                type: "BLOB",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<byte[]>(
                name: "PasswordHash",
                table: "Users",
                type: "BLOB",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<double>(
                name: "DistanceKm",
                table: "Trips",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "decimal(10,2)");

            migrationBuilder.AddColumn<double>(
                name: "AverageSpeed",
                table: "Trips",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FuelUsed",
                table: "Trips",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "TripCost",
                table: "Trips",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateTable(
                name: "GpsData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Latitude = table.Column<double>(type: "REAL", nullable: false),
                    Longitude = table.Column<double>(type: "REAL", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    VehicleId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GpsData", x => x.Id);
                });
        }
    }
}

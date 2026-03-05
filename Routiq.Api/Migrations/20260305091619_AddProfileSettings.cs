using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Routiq.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserProfiles_Users_UserId1",
                table: "UserProfiles");

            migrationBuilder.DropIndex(
                name: "IX_UserProfiles_UserId1",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "UserProfiles");

            migrationBuilder.AddColumn<bool>(
                name: "NotificationsEnabled",
                table: "UserProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PriceAlertsEnabled",
                table: "UserProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TravelStyle",
                table: "UserProfiles",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UnitPreference",
                table: "UserProfiles",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotificationsEnabled",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "PriceAlertsEnabled",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "TravelStyle",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "UnitPreference",
                table: "UserProfiles");

            migrationBuilder.AddColumn<int>(
                name: "UserId1",
                table: "UserProfiles",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_UserId1",
                table: "UserProfiles",
                column: "UserId1",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserProfiles_Users_UserId1",
                table: "UserProfiles",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}

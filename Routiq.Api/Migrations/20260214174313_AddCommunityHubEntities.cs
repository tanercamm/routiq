using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Routiq.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCommunityHubEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Age",
                table: "UserProfiles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                table: "UserProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserPostText",
                table: "TripCheckIns",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DestinationTips",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CityId = table.Column<int>(type: "integer", nullable: false),
                    UserProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Upvotes = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DestinationTips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DestinationTips_Destinations_CityId",
                        column: x => x.CityId,
                        principalTable: "Destinations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DestinationTips_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DestinationTips_CityId",
                table: "DestinationTips",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_DestinationTips_UserProfileId",
                table: "DestinationTips",
                column: "UserProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DestinationTips");

            migrationBuilder.DropColumn(
                name: "Age",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "CountryCode",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "UserPostText",
                table: "TripCheckIns");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Routsky.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupShortlistAndVoting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Origin",
                table: "UserProfiles",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "UserId1",
                table: "UserProfiles",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GroupShortlistRoutes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    DestinationId = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    AddedByUserId = table.Column<int>(type: "integer", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupShortlistRoutes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupShortlistRoutes_TravelGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "TravelGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupShortlistRoutes_Users_AddedByUserId",
                        column: x => x.AddedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RouteVotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupShortlistRouteId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    IsUpvote = table.Column<bool>(type: "boolean", nullable: false),
                    VotedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouteVotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RouteVotes_GroupShortlistRoutes_GroupShortlistRouteId",
                        column: x => x.GroupShortlistRouteId,
                        principalTable: "GroupShortlistRoutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RouteVotes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_UserId1",
                table: "UserProfiles",
                column: "UserId1",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupShortlistRoutes_AddedByUserId",
                table: "GroupShortlistRoutes",
                column: "AddedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupShortlistRoutes_GroupId",
                table: "GroupShortlistRoutes",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteVotes_GroupShortlistRouteId",
                table: "RouteVotes",
                column: "GroupShortlistRouteId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteVotes_UserId",
                table: "RouteVotes",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserProfiles_Users_UserId1",
                table: "UserProfiles",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserProfiles_Users_UserId1",
                table: "UserProfiles");

            migrationBuilder.DropTable(
                name: "RouteVotes");

            migrationBuilder.DropTable(
                name: "GroupShortlistRoutes");

            migrationBuilder.DropIndex(
                name: "IX_UserProfiles_UserId1",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "Origin",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "UserProfiles");
        }
    }
}

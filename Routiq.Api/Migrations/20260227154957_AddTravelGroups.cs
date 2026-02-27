using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Routiq.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTravelGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TravelGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    InviteCode = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TravelGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TravelGroupMembers",
                columns: table => new
                {
                    GroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId1 = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TravelGroupMembers", x => new { x.GroupId, x.UserId });
                    table.ForeignKey(
                        name: "FK_TravelGroupMembers_TravelGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "TravelGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TravelGroupMembers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TravelGroupMembers_Users_UserId1",
                        column: x => x.UserId1,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TravelGroupMembers_UserId",
                table: "TravelGroupMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TravelGroupMembers_UserId1",
                table: "TravelGroupMembers",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_TravelGroups_InviteCode",
                table: "TravelGroups",
                column: "InviteCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TravelGroupMembers");

            migrationBuilder.DropTable(
                name: "TravelGroups");
        }
    }
}

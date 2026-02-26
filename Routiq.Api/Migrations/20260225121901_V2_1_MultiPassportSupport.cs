using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Routiq.Api.Migrations
{
    /// <inheritdoc />
    public partial class V2_1_MultiPassportSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PassportCountryCode",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "PassportCountryCode",
                table: "RouteQueries");

            migrationBuilder.AddColumn<List<string>>(
                name: "Passports",
                table: "UserProfiles",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<List<string>>(
                name: "Passports",
                table: "RouteQueries",
                type: "text[]",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Passports",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "Passports",
                table: "RouteQueries");

            migrationBuilder.AddColumn<string>(
                name: "PassportCountryCode",
                table: "UserProfiles",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PassportCountryCode",
                table: "RouteQueries",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");
        }
    }
}

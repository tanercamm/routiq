using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Routsky.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTravelDatasets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CityIntelligences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CityName = table.Column<string>(type: "text", nullable: false),
                    Country = table.Column<string>(type: "text", nullable: false),
                    SafetyIndex = table.Column<double>(type: "double precision", nullable: false),
                    CostOfLivingIndex = table.Column<double>(type: "double precision", nullable: false),
                    AverageMealCostUSD = table.Column<double>(type: "double precision", nullable: false),
                    AverageTransportCostUSD = table.Column<double>(type: "double precision", nullable: false),
                    BestMonthsToVisit = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CityIntelligences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VisaMatrices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PassportCountry = table.Column<string>(type: "text", nullable: false),
                    DestinationCountry = table.Column<string>(type: "text", nullable: false),
                    VisaStatus = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisaMatrices", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CityIntelligences");

            migrationBuilder.DropTable(
                name: "VisaMatrices");
        }
    }
}

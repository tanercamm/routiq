using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Routiq.Api.Migrations
{
    /// <inheritdoc />
    public partial class V2_InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Destinations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CountryCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Region = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    DailyCostLevel = table.Column<int>(type: "integer", nullable: false),
                    MinRecommendedDays = table.Column<int>(type: "integer", nullable: false),
                    MaxRecommendedDays = table.Column<int>(type: "integer", nullable: false),
                    PopularityWeight = table.Column<double>(type: "double precision", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Destinations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RegionPriceTiers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Region = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    CostLevel = table.Column<int>(type: "integer", nullable: false),
                    DailyBudgetUsdMin = table.Column<int>(type: "integer", nullable: false),
                    DailyBudgetUsdMax = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    LastReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegionPriceTiers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VisaRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PassportCountryCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    DestinationCountryCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Requirement = table.Column<int>(type: "integer", nullable: false),
                    MaxStayDays = table.Column<int>(type: "integer", nullable: false),
                    AvgProcessingDays = table.Column<int>(type: "integer", nullable: false),
                    EVisaUrl = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    LastReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisaRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RouteQueries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    PassportCountryCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    BudgetBracket = table.Column<int>(type: "integer", nullable: false),
                    TotalBudgetUsd = table.Column<int>(type: "integer", nullable: false),
                    DurationDays = table.Column<int>(type: "integer", nullable: false),
                    RegionPreference = table.Column<int>(type: "integer", nullable: false),
                    HasSchengenVisa = table.Column<bool>(type: "boolean", nullable: false),
                    HasUsVisa = table.Column<bool>(type: "boolean", nullable: false),
                    HasUkVisa = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouteQueries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RouteQueries_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Username = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PassportCountryCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    PreferredCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    CountryCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Age = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RouteEliminations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RouteQueryId = table.Column<Guid>(type: "uuid", nullable: false),
                    DestinationId = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<int>(type: "integer", nullable: false),
                    ExplanationText = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouteEliminations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RouteEliminations_Destinations_DestinationId",
                        column: x => x.DestinationId,
                        principalTable: "Destinations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RouteEliminations_RouteQueries_RouteQueryId",
                        column: x => x.RouteQueryId,
                        principalTable: "RouteQueries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SavedRoutes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    RouteQueryId = table.Column<Guid>(type: "uuid", nullable: false),
                    RouteName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SelectionReason = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: false),
                    SavedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserProfileId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedRoutes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavedRoutes_RouteQueries_RouteQueryId",
                        column: x => x.RouteQueryId,
                        principalTable: "RouteQueries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SavedRoutes_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SavedRoutes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RouteStops",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SavedRouteId = table.Column<Guid>(type: "uuid", nullable: false),
                    DestinationId = table.Column<int>(type: "integer", nullable: false),
                    StopOrder = table.Column<int>(type: "integer", nullable: false),
                    RecommendedDays = table.Column<int>(type: "integer", nullable: false),
                    ExpectedCostLevel = table.Column<int>(type: "integer", nullable: false),
                    StopReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouteStops", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RouteStops_Destinations_DestinationId",
                        column: x => x.DestinationId,
                        principalTable: "Destinations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RouteStops_SavedRoutes_SavedRouteId",
                        column: x => x.SavedRouteId,
                        principalTable: "SavedRoutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TraveledRoutes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SavedRouteId = table.Column<Guid>(type: "uuid", nullable: false),
                    TraveledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TransportExpense = table.Column<int>(type: "integer", nullable: false),
                    FoodExpense = table.Column<int>(type: "integer", nullable: false),
                    AccommodationExpense = table.Column<int>(type: "integer", nullable: false),
                    VisaExperience = table.Column<int>(type: "integer", nullable: false),
                    DaySufficiencyJson = table.Column<string>(type: "text", nullable: false),
                    WhyThisRegion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    WhatWasChallenging = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    WhatIWouldDoDifferently = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TraveledRoutes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TraveledRoutes_SavedRoutes_SavedRouteId",
                        column: x => x.SavedRouteId,
                        principalTable: "SavedRoutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RegionPriceTiers_Region_CostLevel",
                table: "RegionPriceTiers",
                columns: new[] { "Region", "CostLevel" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RouteEliminations_DestinationId",
                table: "RouteEliminations",
                column: "DestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteEliminations_RouteQueryId",
                table: "RouteEliminations",
                column: "RouteQueryId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteQueries_UserId",
                table: "RouteQueries",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteStops_DestinationId",
                table: "RouteStops",
                column: "DestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteStops_SavedRouteId_StopOrder",
                table: "RouteStops",
                columns: new[] { "SavedRouteId", "StopOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SavedRoutes_RouteQueryId",
                table: "SavedRoutes",
                column: "RouteQueryId");

            migrationBuilder.CreateIndex(
                name: "IX_SavedRoutes_UserId",
                table: "SavedRoutes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SavedRoutes_UserProfileId",
                table: "SavedRoutes",
                column: "UserProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_TraveledRoutes_SavedRouteId",
                table: "TraveledRoutes",
                column: "SavedRouteId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_UserId",
                table: "UserProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_Username",
                table: "UserProfiles",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VisaRules_PassportCountryCode_DestinationCountryCode",
                table: "VisaRules",
                columns: new[] { "PassportCountryCode", "DestinationCountryCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegionPriceTiers");

            migrationBuilder.DropTable(
                name: "RouteEliminations");

            migrationBuilder.DropTable(
                name: "RouteStops");

            migrationBuilder.DropTable(
                name: "TraveledRoutes");

            migrationBuilder.DropTable(
                name: "VisaRules");

            migrationBuilder.DropTable(
                name: "Destinations");

            migrationBuilder.DropTable(
                name: "SavedRoutes");

            migrationBuilder.DropTable(
                name: "RouteQueries");

            migrationBuilder.DropTable(
                name: "UserProfiles");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}

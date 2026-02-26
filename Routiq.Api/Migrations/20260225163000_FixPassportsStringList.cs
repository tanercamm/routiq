using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Routiq.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixPassportsStringList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The previous migration (V2_1_MultiPassportSupport) added Passports as text[]
            // (native PostgreSQL array). EF Core's ValueConverter now serialises List<string>
            // as a comma-joined plain text string, so we ALTER the column type.
            // USING converts any existing text[] data to comma-joined text.
            migrationBuilder.Sql(
                @"ALTER TABLE ""RouteQueries""
                  ALTER COLUMN ""Passports"" TYPE text
                  USING array_to_string(""Passports"", ',');"
            );

            migrationBuilder.Sql(
                @"ALTER TABLE ""UserProfiles""
                  ALTER COLUMN ""Passports"" TYPE text
                  USING array_to_string(""Passports"", ',');"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert back to text[] (split on comma)
            migrationBuilder.Sql(
                @"ALTER TABLE ""RouteQueries""
                  ALTER COLUMN ""Passports"" TYPE text[]
                  USING string_to_array(""Passports"", ',');"
            );

            migrationBuilder.Sql(
                @"ALTER TABLE ""UserProfiles""
                  ALTER COLUMN ""Passports"" TYPE text[]
                  USING string_to_array(""Passports"", ',');"
            );
        }
    }
}

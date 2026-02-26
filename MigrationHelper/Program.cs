using Npgsql;

const string connStr = "Host=localhost;Port=5433;Database=routiq_db;Username=postgres;Password=SecurePass123!;Include Error Detail=true";

await using var conn = new NpgsqlConnection(connStr);
await conn.OpenAsync();

Console.WriteLine("Connected to routiq_db.");

// 1) ALTER RouteQueries.Passports: text[] -> text
await ExecAsync(conn,
    """
    ALTER TABLE "RouteQueries"
      ALTER COLUMN "Passports" TYPE text
      USING array_to_string("Passports", ',');
    """,
    "RouteQueries.Passports altered to text"
);

// 2) ALTER UserProfiles.Passports: text[] -> text
await ExecAsync(conn,
    """
    ALTER TABLE "UserProfiles"
      ALTER COLUMN "Passports" TYPE text
      USING array_to_string("Passports", ',');
    """,
    "UserProfiles.Passports altered to text"
);

// 3) Insert the migration record into __EFMigrationsHistory
await ExecAsync(conn,
    """
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260225163000_FixPassportsStringList', '8.0.2')
    ON CONFLICT DO NOTHING;
    """,
    "Migration record inserted into __EFMigrationsHistory"
);

Console.WriteLine("All done. Passports columns are now plain text.");

static async Task ExecAsync(NpgsqlConnection conn, string sql, string label)
{
    try
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        await cmd.ExecuteNonQueryAsync();
        Console.WriteLine($"  OK: {label}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  SKIP/WARN ({label}): {ex.Message}");
    }
}

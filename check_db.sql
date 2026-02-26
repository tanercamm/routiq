-- Check applied migrations
SELECT "MigrationId" FROM "__EFMigrationsHistory" ORDER BY "MigrationId";

-- Check what columns RouteQueries actually has
SELECT column_name, data_type FROM information_schema.columns
WHERE table_name = 'RouteQueries'
ORDER BY ordinal_position;

-- Check what columns UserProfiles actually has
SELECT column_name, data_type FROM information_schema.columns
WHERE table_name = 'UserProfiles'
ORDER BY ordinal_position;

namespace ATMS.Admin.Data.Infrastructure.Migrations;

public interface IMigrationRunner
{
    Task<MigrationResult> MigrateUpAsync(CancellationToken ct);
    Task<MigrationResult> MigrateDownAsync(string targetMigration, CancellationToken ct);
}

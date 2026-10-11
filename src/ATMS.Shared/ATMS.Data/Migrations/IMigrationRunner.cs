namespace ATMS.Data.Migrations;

public interface IMigrationRunner
{
    Task<MigrationResult> MigrateUpAsync(CancellationToken cancellationToken);
    Task<MigrationResult> MigrateDownAsync(string targetMigration, CancellationToken cancellationToken);
    Task MigrateIfEmptyAsync(CancellationToken cancellationToken);
}

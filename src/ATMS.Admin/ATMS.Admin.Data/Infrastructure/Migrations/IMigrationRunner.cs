namespace ATMS.Admin.Data.Infrastructure.Migrations;

public interface IMigrationRunner
{
    Task<MigrationResult> MigrateUpAsync(CancellationToken cancellationToken);
    Task<MigrationResult> MigrateDownAsync(string targetMigration, CancellationToken cancellationToken);
}

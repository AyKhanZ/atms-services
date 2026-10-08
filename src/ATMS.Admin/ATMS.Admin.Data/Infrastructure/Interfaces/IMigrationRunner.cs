using ATMS.Admin.Data.Models;

namespace ATMS.Admin.Data.Infrastructure.Interfaces;

public interface IMigrationRunner
{
    Task<MigrationResult> MigrateUpAsync(CancellationToken cancellationToken);
    Task<MigrationResult> MigrateDownAsync(string targetMigration, CancellationToken cancellationToken);
}

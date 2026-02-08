using Microsoft.EntityFrameworkCore;

namespace ATMS.Admin.Data.Infrastructure.Migrations;

public class MigrationRunner<TDbContext>(TDbContext context) : IMigrationRunner
    where TDbContext : DbContext
{
    public async Task<MigrationResult> MigrateUpAsync(CancellationToken cancellationToken)
    {
        try
        {
            var pending = (await context.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();

            await context.Database.MigrateAsync(cancellationToken);

            return new MigrationResult
            {
                AppliedMigrations = pending
            };
        }
        catch (Exception ex)
        {
            return new MigrationResult
            {
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<MigrationResult> MigrateDownAsync(string targetMigration, CancellationToken cancellationToken)
    {
        try
        {
            await context.Database.MigrateAsync(targetMigration, cancellationToken);

            return new MigrationResult
            {
                RolledBackMigration = targetMigration
            };
        }
        catch (Exception ex)
        {
            return new MigrationResult
            {
                ErrorMessage = ex.Message
            };
        }
    }
}

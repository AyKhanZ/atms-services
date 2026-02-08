using ATMS.Admin.Contracts.Commands.Migration;
using ATMS.Admin.Contracts.Models;
using ATMS.Admin.Data.Infrastructure.Migrations;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Migratons;

public class MigrationsHandler(IMigrationRunner migrationRunner)
    : IRequestHandler<ApplyMigrationsCommand, MigrationModel>
      , IRequestHandler<DownMigrationCommand, MigrationModel>
{
    public async Task<MigrationModel> Handle(ApplyMigrationsCommand command, CancellationToken cancellationToken)
    {
        var result = await migrationRunner
            .MigrateUpAsync(cancellationToken);

        return new MigrationModel
        {
            ErrorMessage = result.ErrorMessage,
            AppliedMigrations = [.. result.AppliedMigrations],
        };
    }

    public async Task<MigrationModel> Handle(DownMigrationCommand command, CancellationToken cancellationToken)
    {
        var result = await migrationRunner
            .MigrateDownAsync(command.MigrationName, cancellationToken);

        return new MigrationModel
        {
            ErrorMessage = result.ErrorMessage,
            RolledBackMigration = result.RolledBackMigration,
        };
    }
}

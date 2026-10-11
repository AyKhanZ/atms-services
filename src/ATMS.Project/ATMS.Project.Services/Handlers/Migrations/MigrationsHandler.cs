using ATMS.Project.Contracts.Commands.Migration;
using ATMS.Project.Contracts.Models;
using ATMS.Data.Migrations;
using MediatR;

namespace ATMS.Project.Services.Handlers.Migrations;

public sealed class MigrationsHandler(IMigrationRunner migrationRunner)
    : IRequestHandler<ApplyMigrationsCommand, MigrationModel>
      , IRequestHandler<DownMigrationCommand, MigrationModel>
{
    public async Task<MigrationModel> Handle(ApplyMigrationsCommand command, CancellationToken cancellationToken)
    {
        var result = await migrationRunner.MigrateUpAsync(cancellationToken);

        return new MigrationModel
        {
            ErrorMessage = result.ErrorMessage,
            AppliedMigrations = [.. result.AppliedMigrations]
        };
    }

    public async Task<MigrationModel> Handle(DownMigrationCommand command, CancellationToken cancellationToken)
    {
        var result = await migrationRunner.MigrateDownAsync(command.MigrationName, cancellationToken);

        return new MigrationModel
        {
            ErrorMessage = result.ErrorMessage,
            RolledBackMigration = result.RolledBackMigration
        };
    }
}

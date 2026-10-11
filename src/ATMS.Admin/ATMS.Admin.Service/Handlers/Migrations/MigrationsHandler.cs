using ATMS.Admin.Contracts.Commands.Migration;
using ATMS.Admin.Contracts.Models;
using ATMS.Admin.Service.Providers.Interfaces;
using ATMS.Data.Migrations;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Migrations;

public sealed class MigrationsHandler(
    IMigrationRunner migrationRunner,
    IMigrationsProvider migrationsProvider)
    : IRequestHandler<ApplyMigrationsCommand, ApplyMigrationsModel>
      , IRequestHandler<DownMigrationCommand, MigrationModel>
{
    public async Task<ApplyMigrationsModel> Handle(
        ApplyMigrationsCommand command, CancellationToken cancellationToken)
    {
        var adminResult = await migrationRunner.MigrateUpAsync(cancellationToken);
        var admin = ToModel(adminResult);

        if (!admin.Success)
        {
            return new ApplyMigrationsModel
            {
                Admin = admin
            };
        }

        var project = await migrationsProvider.ApplyAsync(cancellationToken);

        return new ApplyMigrationsModel
        {
            Admin = admin,
            Project = project
        };
    }

    public async Task<MigrationModel> Handle(DownMigrationCommand command, CancellationToken cancellationToken)
    {
        var result = await migrationRunner.MigrateDownAsync(command.MigrationName, cancellationToken);

        return ToModel(result);
    }

    private static MigrationModel ToModel(MigrationResult result) => new()
    {
        ErrorMessage = result.ErrorMessage,
        AppliedMigrations = [.. result.AppliedMigrations],
        RolledBackMigration = result.RolledBackMigration
    };
}

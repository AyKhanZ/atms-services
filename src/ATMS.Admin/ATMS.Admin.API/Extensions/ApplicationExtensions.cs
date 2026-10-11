using ATMS.Admin.Service.Infrastructure.Interfaces;
using ATMS.Data.Migrations;

namespace ATMS.Admin.API.Extensions;

public static class ApplicationExtensions
{
    public static async Task InitializeDataAsync(this IHost app)
    {
        using var scope = app.Services.CreateScope();

        var migrationRunner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        await migrationRunner.MigrateIfEmptyAsync(CancellationToken.None);

        var initializer = scope.ServiceProvider.GetRequiredService<IDataInitializer>();
        await initializer.InitializeAsync();
    }
}

using ATMS.Data.Migrations;

namespace ATMS.Project.API.Extensions;

public static class ApplicationExtensions
{
    public static async Task InitializeDataAsync(this IHost app)
    {
        using var scope = app.Services.CreateScope();
        var migrationRunner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        await migrationRunner.MigrateIfEmptyAsync(CancellationToken.None);
    }
}

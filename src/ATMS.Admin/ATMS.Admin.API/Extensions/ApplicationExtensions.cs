using ATMS.Admin.Service.Infrastructure.Interfaces;

namespace ATMS.Admin.API.Extensions;

public static class ApplicationExtensions
{
    public static async Task InitializeDataAsync(this IHost app)
    {
        using var scope = app.Services.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<IDataInitializer>();
        await initializer.InitializeAsync();
    }
}

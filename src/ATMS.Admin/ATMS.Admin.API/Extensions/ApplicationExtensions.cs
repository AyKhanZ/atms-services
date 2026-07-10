using ATMS.Admin.Service.Infrastructure.Interfaces;
using ATMS.Messaging.Infrastructure;

namespace ATMS.Admin.API.Extensions;

public static class ApplicationExtensions
{
    public static async Task InitializeDataAsync(this IHost app)
    {
        using var scope = app.Services.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<IDataInitializer>();
        await initializer.InitializeAsync();
    }
    
    public static async Task InitializeEventBusAsync(this IHost app)
    {
        var messagingConstantsInitializer = app.Services
            .GetRequiredService<MessagingInitializer>();
        await messagingConstantsInitializer.InitializeAsync();
    }
}

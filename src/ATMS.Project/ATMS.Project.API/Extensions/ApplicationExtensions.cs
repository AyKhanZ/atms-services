using ATMS.Messaging.Infrastructure;

namespace ATMS.Project.API.Extensions;

public static class ApplicationExtensions
{
    public static async Task InitializeEventBusAsync(this IHost app)
    {
        var messagingConstantsInitializer = app.Services
            .GetRequiredService<MessagingConstantsInitializer>();
        await messagingConstantsInitializer.InitializeAsync();
    }
}
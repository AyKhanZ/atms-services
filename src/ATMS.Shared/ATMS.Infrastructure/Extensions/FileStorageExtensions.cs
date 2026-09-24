using ATMS.Infrastructure.Files;
using Microsoft.Extensions.DependencyInjection;

namespace ATMS.Infrastructure.Extensions;

public static class FileStorageExtensions
{
    public static IServiceCollection AddLocalFileStorage(this IServiceCollection services)
    {
        services.AddScoped<IFileStorage, LocalFileStorage>();
        services.AddSingleton<IFileSignatureService, FileSignatureService>();
        return services;
    }
}

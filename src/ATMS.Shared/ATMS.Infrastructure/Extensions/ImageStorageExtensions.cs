using ATMS.Infrastructure.Images;
using ATMS.Infrastructure.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace ATMS.Infrastructure.Extensions;

public static class ImageStorageExtensions
{
    public static IServiceCollection AddLocalImageStorage(this IServiceCollection services)
    {
        services.AddScoped<IImageStorage, LocalImageStorage>();
        services.AddScoped<IImageUrlBuilder, LocalImageUrlBuilder>();
        return services;
    }

    public static IApplicationBuilder UseLocalImageFiles(
        this IApplicationBuilder app,
        IConfiguration configuration)
    {
        var imagesOptions = configuration
            .GetSection(nameof(ImagesOptions))
            .Get<ImagesOptions>();

        if (imagesOptions is null)
        {
            return app;
        }

        Directory.CreateDirectory(imagesOptions.ImagesRootPath);

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(imagesOptions.ImagesRootPath),
            RequestPath = new PathString(new Uri(imagesOptions.BaseImageUrl).AbsolutePath)
        });

        return app;
    }
}

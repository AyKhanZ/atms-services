using ATMS.Infrastructure.Options;
using Microsoft.Extensions.Configuration;

namespace ATMS.Infrastructure.Images;

public sealed class LocalImageUrlBuilder(IConfiguration configuration) : IImageUrlBuilder
{
    private readonly ImagesOptions _options =
        configuration.GetSection(nameof(ImagesOptions)).Get<ImagesOptions>()
        ?? throw new InvalidOperationException($"{nameof(ImagesOptions)} section is not configured.");

    public string? BuildUrl(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return null;
        }

        return $"{_options.BaseImageUrl.TrimEnd('/')}/{relativePath}";
    }
}

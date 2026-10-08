using ATMS.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace ATMS.Infrastructure.Images;

public sealed class LocalImageUrlBuilder(IOptions<ImagesOptions> options) : IImageUrlBuilder
{
    private readonly ImagesOptions _options = options.Value;

    public string? BuildUrl(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return null;
        }

        return $"{_options.BaseImageUrl.TrimEnd('/')}/{relativePath}";
    }
}

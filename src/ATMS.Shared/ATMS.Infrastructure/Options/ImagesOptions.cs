namespace ATMS.Infrastructure.Options;

public class ImagesOptions
{
    public required string ImagesRootPath { get; init; }
    public required string BaseImageUrl { get; init; }
    public long MaxFileSizeBytes { get; init; } = 5 * 1024 * 1024;
    public long MaxPixelCount { get; init; } = 12_000_000;
    public string[] AllowedContentTypes { get; init; } =
    [
        "image/jpeg",
        "image/jfif",
        "image/png",
        "image/webp"
    ];
}

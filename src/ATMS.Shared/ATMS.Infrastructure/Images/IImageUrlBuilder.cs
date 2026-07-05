namespace ATMS.Infrastructure.Images;

public interface IImageUrlBuilder
{
    string? BuildUrl(string? relativePath);
}

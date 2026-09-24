using Microsoft.AspNetCore.Http;

namespace ATMS.Infrastructure.Files;

public interface IFileSignatureService
{
    bool IsAllowedExtension(string? extension);

    string GetContentType(string extension);

    bool CanPreview(string contentType);

    Task<bool> MatchesExtensionAsync(IFormFile file, string extension, CancellationToken cancellationToken);
}

using Microsoft.AspNetCore.Http;

namespace ATMS.Infrastructure.Files;

public interface IFileStorage
{
    Task<string> SaveAsync(
        IFormFile file,
        string directory,
        string extension,
        CancellationToken cancellationToken);

    string GetFullPath(string relativePath);

    Task DeleteAsync(string? relativePath, CancellationToken cancellationToken);

    Task<bool> IsWritableAsync(CancellationToken cancellationToken);
}

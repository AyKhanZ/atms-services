using Microsoft.AspNetCore.Http;

namespace ATMS.Infrastructure.Images;

public interface IImageStorage
{
    Task<StoredImage> SaveAsync(
        IFormFile file,
        ImageStorageFolder folder,
        Guid ownerId,
        CancellationToken cancellationToken);

    Task DeleteAsync(string? relativePath, CancellationToken cancellationToken);
}

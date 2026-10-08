using Microsoft.AspNetCore.Http;
using ATMS.Infrastructure.Enums;

namespace ATMS.Infrastructure.Images;

public interface IImageStorage
{
    Task<StoredImage> SaveAsync(
        IFormFile file,
        ImageStorageFolderEnum folder,
        Guid ownerId,
        CancellationToken cancellationToken);

    Task DeleteAsync(string? relativePath, CancellationToken cancellationToken);
}

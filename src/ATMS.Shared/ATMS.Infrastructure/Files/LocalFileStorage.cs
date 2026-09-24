using ATMS.Application.Exceptions.Configuration;
using ATMS.Application.Exceptions.Resources;
using ATMS.Infrastructure.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace ATMS.Infrastructure.Files;

// Files are kept byte for byte: unlike avatars, an attachment must download exactly as uploaded.
// The root is never served as static files; every read goes through an authorized endpoint.
public sealed class LocalFileStorage(IConfiguration configuration) : IFileStorage
{
    private readonly AttachmentsOptions _options =
        configuration.GetSection(nameof(AttachmentsOptions)).Get<AttachmentsOptions>()
        ?? throw new ConfigurationException(ConfigurationErrorType.AttachmentsSectionNotFound,
            string.Format(LogMessages.ConfigSectionNotFound, nameof(AttachmentsOptions)));

    public async Task<string> SaveAsync(
        IFormFile file,
        string directory,
        string extension,
        CancellationToken cancellationToken)
    {
        var fileName = $"{Guid.NewGuid():N}.{extension.ToLowerInvariant()}";
        var relativePath = $"{directory.Trim('/')}/{fileName}";
        var destinationPath = GetFullPath(relativePath);
        var destinationDirectory = Path.GetDirectoryName(destinationPath)!;
        Directory.CreateDirectory(destinationDirectory);

        // Written next to the destination and moved in one step, so a half-written file never
        // appears under its final name.
        var tempPath = Path.Combine(destinationDirectory, $"{Guid.NewGuid():N}.tmp");

        try
        {
            await using (var output = File.Create(tempPath))
            {
                await file.CopyToAsync(output, cancellationToken);
            }

            File.Move(tempPath, destinationPath);
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }

        return relativePath;
    }

    public string GetFullPath(string relativePath)
    {
        var normalized = relativePath.Replace('/', Path.DirectorySeparatorChar);
        var fullRoot = Path.GetFullPath(_options.RootPath);
        var rootWithSeparator = Path.EndsInDirectorySeparator(fullRoot)
            ? fullRoot
            : fullRoot + Path.DirectorySeparatorChar;
        var fullPath = Path.GetFullPath(Path.Combine(fullRoot, normalized));

        if (!fullPath.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The file path leaves the attachments root.");
        }

        return fullPath;
    }

    public Task DeleteAsync(string? relativePath, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return Task.CompletedTask;
        }

        var fullPath = GetFullPath(relativePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }
}

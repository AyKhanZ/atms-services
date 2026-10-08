using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using ATMS.Application.Exceptions.Image;
using ATMS.Infrastructure.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace ATMS.Infrastructure.Images;

public sealed class LocalImageStorage(
    IConfiguration configuration,
    IImageUrlBuilder imageUrlBuilder) : IImageStorage
{
    private const string ValidationPropertyName = "Image";

    // Only the formats we accept can be decoded, so a flaw in another decoder (TIFF, GIF, BMP...) is
    // out of reach. One frame: an animated image is stored as its first frame and cannot multiply memory.
    private static readonly DecoderOptions DecoderOptions = new()
    {
        Configuration = new Configuration(
            new JpegConfigurationModule(),
            new PngConfigurationModule(),
            new WebpConfigurationModule()),
        MaxFrames = 1
    };

    private readonly ImagesOptions _options =
        configuration.GetSection(nameof(ImagesOptions)).Get<ImagesOptions>()
        ?? throw new InvalidOperationException($"{nameof(ImagesOptions)} section is not configured.");

    public async Task<StoredImage> SaveAsync(
        IFormFile file,
        ImageStorageFolder folder,
        Guid ownerId,
        CancellationToken cancellationToken)
    {
        ValidateEnvelope(file);

        var imageKind = await DetectImageKindAsync(file, cancellationToken);
        ValidateContentType(file, imageKind);

        var folderName = ToFolderName(folder);
        var fileName = $"{ownerId:N}-{Guid.NewGuid():N}{imageKind.Extension}";
        var relativePath = $"{folderName}/{fileName}";
        var directory = Path.Combine(_options.ImagesRootPath, folderName);
        Directory.CreateDirectory(directory);

        var destinationPath = Path.Combine(directory, fileName);
        var tempPath = Path.Combine(directory, $"{ownerId:N}.{Guid.NewGuid():N}.tmp");

        try
        {
            // Only the header is read here: a few-megabyte PNG can declare a canvas that takes gigabytes
            // once decoded, so the size is refused before decoding.
            await using (var header = file.OpenReadStream())
            {
                var info = await Image.IdentifyAsync(DecoderOptions, header, cancellationToken);
                var pixelCount = (long)info.Width * info.Height;
                if (pixelCount > _options.MaxPixelCount)
                {
                    ThrowImageValidation(
                        "Image dimensions are too large.",
                        $"Image dimensions are too large. Maximum pixel count is {_options.MaxPixelCount}.");
                }
            }

            await using var input = file.OpenReadStream();
            using var image = await Image.LoadAsync(DecoderOptions, input, cancellationToken);

            // The orientation lives in EXIF, which is removed below: a phone photo would end up on its side.
            image.Mutate(context => context.AutoOrient());

            image.Metadata.ExifProfile = null;
            image.Metadata.IccProfile = null;
            image.Metadata.XmpProfile = null;

            await SaveWithoutMetadataAsync(image, imageKind, tempPath, cancellationToken);
            File.Move(tempPath, destinationPath, overwrite: true);
        }
        catch (UnknownImageFormatException)
        {
            ThrowImageValidation("Invalid image file.", "Unsupported or invalid image file.");
        }
        catch (InvalidImageContentException)
        {
            ThrowImageValidation("Invalid image file.", "Unsupported or invalid image file.");
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }

        var size = new FileInfo(destinationPath).Length;

        return new StoredImage(
            relativePath,
            imageUrlBuilder.BuildUrl(relativePath)!,
            imageKind.ContentType,
            size);
    }

    public Task DeleteAsync(string? relativePath, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return Task.CompletedTask;
        }

        var fullPath = GetSafeFullPath(relativePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    private static string ToFolderName(ImageStorageFolder folder) =>
        folder switch
        {
            ImageStorageFolder.Users => "users",
            ImageStorageFolder.Organizations => "organizations",
            ImageStorageFolder.Projects => "projects",
            ImageStorageFolder.Tickets => "tickets",
            ImageStorageFolder.Tasks => "tasks",
            _ => throw new ArgumentOutOfRangeException(nameof(folder), folder, null)
        };

    private static async Task SaveWithoutMetadataAsync(
        Image image,
        ImageKind imageKind,
        string path,
        CancellationToken cancellationToken)
    {
        switch (imageKind.ContentType)
        {
            case "image/jpeg":
                await image.SaveAsJpegAsync(path, cancellationToken);
                break;
            case "image/png":
                await image.SaveAsPngAsync(path, cancellationToken);
                break;
            case "image/webp":
                await image.SaveAsWebpAsync(path, cancellationToken);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(imageKind), imageKind.ContentType, null);
        }
    }

    private void ValidateEnvelope(IFormFile file)
    {
        if (file is null)
        {
            ThrowImageValidation("Image file is required.", "Image file is required.");
        }

        if (file.Length == 0)
        {
            ThrowImageValidation("Image file is required.", "Image file is empty.");
        }

        if (file.Length > _options.MaxFileSizeBytes)
        {
            ThrowImageValidation(
                "Image file is too large.",
                $"Image size must not exceed {_options.MaxFileSizeBytes} bytes.");
        }

        if (string.IsNullOrWhiteSpace(file.ContentType))
        {
            ThrowImageValidation("Unsupported image format.", "Image content type is required.");
        }
    }

    private void ValidateContentType(IFormFile file, ImageKind imageKind)
    {
        var allowed = _options.AllowedContentTypes
            .Any(contentType => string.Equals(contentType, imageKind.ContentType, StringComparison.OrdinalIgnoreCase));

        if (!allowed)
        {
            ThrowImageValidation("Unsupported image format.", "Unsupported image type.");
        }

        if (!imageKind.AcceptedContentTypes.Any(contentType =>
                string.Equals(file.ContentType, contentType, StringComparison.OrdinalIgnoreCase)))
        {
            ThrowImageValidation("Unsupported image format.", "Image content type does not match the file content.");
        }
    }

    private static async Task<ImageKind> DetectImageKindAsync(IFormFile file, CancellationToken cancellationToken)
    {
        var buffer = new byte[12];
        await using var stream = file.OpenReadStream();
        var bytesRead = await stream.ReadAsync(buffer, cancellationToken);

        if (bytesRead >= 3 && buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF)
        {
            var isJfif = bytesRead >= 11 &&
                         buffer[6] == 0x4A &&
                         buffer[7] == 0x46 &&
                         buffer[8] == 0x49 &&
                         buffer[9] == 0x46 &&
                         buffer[10] == 0x00;

            return isJfif
                ? new ImageKind(".jfif", "image/jpeg", ["image/jpeg", "image/jfif"])
                : new ImageKind(".jpg", "image/jpeg", ["image/jpeg", "image/jfif"]);
        }

        if (bytesRead >= 8 &&
            buffer[0] == 0x89 &&
            buffer[1] == 0x50 &&
            buffer[2] == 0x4E &&
            buffer[3] == 0x47 &&
            buffer[4] == 0x0D &&
            buffer[5] == 0x0A &&
            buffer[6] == 0x1A &&
            buffer[7] == 0x0A)
        {
            return new ImageKind(".png", "image/png", ["image/png"]);
        }

        if (bytesRead >= 12 &&
            buffer[0] == 0x52 &&
            buffer[1] == 0x49 &&
            buffer[2] == 0x46 &&
            buffer[3] == 0x46 &&
            buffer[8] == 0x57 &&
            buffer[9] == 0x45 &&
            buffer[10] == 0x42 &&
            buffer[11] == 0x50)
        {
            return new ImageKind(".webp", "image/webp", ["image/webp"]);
        }

        ThrowImageValidation("Unsupported image format.", "Unsupported image type.");
        throw new UnreachableException();
    }

    private string GetSafeFullPath(string relativePath)
    {
        var normalized = relativePath.Replace('/', Path.DirectorySeparatorChar);
        var fullRoot = Path.GetFullPath(_options.ImagesRootPath);
        var rootWithSeparator = Path.EndsInDirectorySeparator(fullRoot)
            ? fullRoot
            : fullRoot + Path.DirectorySeparatorChar;
        var fullPath = Path.GetFullPath(Path.Combine(fullRoot, normalized));

        if (!fullPath.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
        {
            ThrowImageValidation("Invalid image path.", "Invalid image path.");
        }

        return fullPath;
    }

    [DoesNotReturn]
    private static void ThrowImageValidation(string userMessage, string logMessage)
    {
        throw new ImageException(
            ImageErrorType.Validation,
            userMessage,
            logMessage,
            ValidationPropertyName);
    }

    private sealed record ImageKind(
        string Extension,
        string ContentType,
        string[] AcceptedContentTypes);
}

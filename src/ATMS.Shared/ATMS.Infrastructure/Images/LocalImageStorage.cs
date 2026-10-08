using System.Diagnostics;
using ATMS.Application.Exceptions.Enums;
using ATMS.Application.Exceptions.Image;
using ATMS.Infrastructure.Enums;
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

    // only our formats can be decoded (no TIFF/GIF/BMP decoder bugs), only the first frame of an animation
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
        ImageStorageFolderEnum folder,
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
            // check the size from the header first: a small png can decode into gigabytes
            await using (var header = file.OpenReadStream())
            {
                var info = await Image.IdentifyAsync(DecoderOptions, header, cancellationToken);
                var pixelCount = (long)info.Width * info.Height;
                if (pixelCount > _options.MaxPixelCount)
                {
                    throw InvalidImage(
                        "Image dimensions are too large.",
                        $"Image dimensions are too large. Maximum pixel count is {_options.MaxPixelCount}.");
                }
            }

            await using var input = file.OpenReadStream();
            using var image = await Image.LoadAsync(DecoderOptions, input, cancellationToken);

            // EXIF is removed below, so rotate first or phone photos end up sideways
            image.Mutate(context => context.AutoOrient());

            image.Metadata.ExifProfile = null;
            image.Metadata.IccProfile = null;
            image.Metadata.XmpProfile = null;

            await SaveWithoutMetadataAsync(image, imageKind, tempPath, cancellationToken);
            File.Move(tempPath, destinationPath, overwrite: true);
        }
        catch (UnknownImageFormatException)
        {
            throw InvalidImage("Invalid image file.", "Unsupported or invalid image file.");
        }
        catch (InvalidImageContentException)
        {
            throw InvalidImage("Invalid image file.", "Unsupported or invalid image file.");
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

    private static string ToFolderName(ImageStorageFolderEnum folder) =>
        folder switch
        {
            ImageStorageFolderEnum.Users => "users",
            ImageStorageFolderEnum.Organizations => "organizations",
            ImageStorageFolderEnum.Projects => "projects",
            ImageStorageFolderEnum.Tickets => "tickets",
            ImageStorageFolderEnum.Tasks => "tasks",
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
            throw InvalidImage("Image file is required.", "Image file is required.");
        }

        if (file.Length == 0)
        {
            throw InvalidImage("Image file is required.", "Image file is empty.");
        }

        if (file.Length > _options.MaxFileSizeBytes)
        {
            throw InvalidImage(
                "Image file is too large.",
                $"Image size must not exceed {_options.MaxFileSizeBytes} bytes.");
        }

        if (string.IsNullOrWhiteSpace(file.ContentType))
        {
            throw InvalidImage("Unsupported image format.", "Image content type is required.");
        }
    }

    private void ValidateContentType(IFormFile file, ImageKind imageKind)
    {
        var allowed = _options.AllowedContentTypes
            .Any(contentType => string.Equals(contentType, imageKind.ContentType, StringComparison.OrdinalIgnoreCase));

        if (!allowed)
        {
            throw InvalidImage("Unsupported image format.", "Unsupported image type.");
        }

        if (!imageKind.AcceptedContentTypes.Any(contentType =>
                string.Equals(file.ContentType, contentType, StringComparison.OrdinalIgnoreCase)))
        {
            throw InvalidImage("Unsupported image format.", "Image content type does not match the file content.");
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

        throw InvalidImage("Unsupported image format.", "Unsupported image type.");
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
            throw InvalidImage("Invalid image path.", "Invalid image path.");
        }

        return fullPath;
    }

    private static ImageException InvalidImage(string userMessage, string logMessage) =>
        new(ImageErrorTypeEnum.Validation, userMessage, logMessage, ValidationPropertyName);

    private sealed record ImageKind(
        string Extension,
        string ContentType,
        string[] AcceptedContentTypes);
}

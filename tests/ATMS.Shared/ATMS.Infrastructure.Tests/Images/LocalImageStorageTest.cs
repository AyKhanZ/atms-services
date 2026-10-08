using ATMS.Application.Exceptions.Image;
using ATMS.Infrastructure.Enums;
using ATMS.Infrastructure.Images;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.PixelFormats;

namespace ATMS.Infrastructure.Tests.Images;

public sealed class LocalImageStorageTest : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"atms-images-{Guid.NewGuid():N}");
    private readonly LocalImageStorage _storage;

    public LocalImageStorageTest()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ImagesOptions:ImagesRootPath"] = _root,
                ["ImagesOptions:BaseImageUrl"] = "http://localhost/app/images"
            })
            .Build();
        var urlBuilder = new Mock<IImageUrlBuilder>();
        urlBuilder.Setup(x => x.BuildUrl(It.IsAny<string?>())).Returns<string?>(path => path);

        _storage = new LocalImageStorage(configuration, urlBuilder.Object);
    }

    // A one-colour PNG of 25 million pixels is a few kilobytes on disk and about 100 MB once decoded.
    [Fact]
    public async Task SaveAsync_WhenCanvasIsTooLarge_RefusesIt()
    {
        using var huge = new Image<L8>(5000, 5000);
        var file = await ToFormFileAsync(huge, "image/png", (image, stream) => image.SaveAsPngAsync(stream));

        var exception = await Assert.ThrowsAsync<ImageException>(
            () => _storage.SaveAsync(file, ImageStorageFolderEnum.Users, Guid.NewGuid(), CancellationToken.None));

        Assert.Equal("Image dimensions are too large.", exception.UserMessage);
    }

    // A phone stores the photo sideways and says "turn it" in EXIF; EXIF is removed, so the turn is applied.
    [Fact]
    public async Task SaveAsync_WhenExifSaysRotate_StoresTheImageUpright()
    {
        using var photo = new Image<Rgb24>(40, 20);
        photo.Metadata.ExifProfile = new ExifProfile();
        photo.Metadata.ExifProfile.SetValue(ExifTag.Orientation, (ushort)6);
        var file = await ToFormFileAsync(photo, "image/jpeg", (image, stream) => image.SaveAsJpegAsync(stream));

        var stored = await _storage.SaveAsync(file, ImageStorageFolderEnum.Users, Guid.NewGuid(), CancellationToken.None);

        using var saved = await Image.LoadAsync(Path.Combine(_root, stored.RelativePath));
        Assert.Equal(20, saved.Width);
        Assert.Equal(40, saved.Height);
        Assert.Null(saved.Metadata.ExifProfile);
    }

    // TIFF is not accepted; its decoder must not even be reached.
    [Fact]
    public async Task SaveAsync_WhenFileIsTiff_RefusesIt()
    {
        using var tiff = new Image<Rgb24>(10, 10);
        var file = await ToFormFileAsync(tiff, "image/tiff", (image, stream) => image.SaveAsTiffAsync(stream));

        var exception = await Assert.ThrowsAsync<ImageException>(
            () => _storage.SaveAsync(file, ImageStorageFolderEnum.Users, Guid.NewGuid(), CancellationToken.None));

        Assert.Equal("Unsupported image format.", exception.UserMessage);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    private static async Task<IFormFile> ToFormFileAsync<TPixel>(
        Image<TPixel> image,
        string contentType,
        Func<Image<TPixel>, Stream, Task> save)
        where TPixel : unmanaged, IPixel<TPixel>
    {
        var stream = new MemoryStream();
        await save(image, stream);
        stream.Position = 0;

        return new FormFile(stream, 0, stream.Length, "file", "image")
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }
}

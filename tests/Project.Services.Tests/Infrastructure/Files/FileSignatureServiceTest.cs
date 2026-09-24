using System.Text;
using ATMS.Infrastructure.Files;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Project.Services.Tests.Infrastructure.Files;

public class FileSignatureServiceTest
{
    private static readonly byte[] Pdf = "%PDF-1.7\n"u8.ToArray();
    private static readonly byte[] Zip = [0x50, 0x4B, 0x03, 0x04, 0x14, 0x00, 0x06, 0x00];
    private static readonly byte[] Ole = [0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1, 0x00, 0x00];
    private static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10];
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00];
    private static readonly byte[] Gif = "GIF89a"u8.ToArray();
    private static readonly byte[] Webp = "RIFF\0\0\0\0WEBPVP8 "u8.ToArray();
    private static readonly byte[] Text = Encoding.UTF8.GetBytes("Номер;Сумма\n1;200\n");
    private static readonly byte[] Exe = [0x4D, 0x5A, 0x90, 0x00, 0x03, 0x00];

    private static FileSignatureService Service(params string[] allowedExtensions)
    {
        var settings = new Dictionary<string, string?> { ["AttachmentsOptions:RootPath"] = "attachments" };
        for (var index = 0; index < allowedExtensions.Length; index++)
        {
            settings[$"AttachmentsOptions:AllowedExtensions:{index}"] = allowedExtensions[index];
        }

        return new FileSignatureService(new ConfigurationBuilder().AddInMemoryCollection(settings).Build());
    }

    private static IFormFile File(byte[] content, string name)
    {
        return new FormFile(new MemoryStream(content), 0, content.Length, "file", name);
    }

    public static TheoryData<string, byte[]> MatchingFiles => new()
    {
        { "pdf", Pdf },
        { "docx", Zip },
        { "xlsx", Zip },
        { "pptx", Zip },
        { "odt", Zip },
        { "zip", Zip },
        { "doc", Ole },
        { "xls", Ole },
        { "ppt", Ole },
        { "jpg", Jpeg },
        { "jpeg", Jpeg },
        { "png", Png },
        { "gif", Gif },
        { "webp", Webp },
        { "txt", Text },
        { "csv", Text },
        { "txt", [0xFF, 0xFE, 0x41, 0x00] }
    };

    [Theory]
    [MemberData(nameof(MatchingFiles))]
    public async Task MatchesExtensionAsync_WhenBytesAgreeWithExtension_ReturnsTrue(string extension, byte[] content)
    {
        var matches = await Service().MatchesExtensionAsync(File(content, $"file.{extension}"), extension, CancellationToken.None);

        Assert.True(matches);
    }

    public static TheoryData<string, byte[]> MismatchedFiles => new()
    {
        { "pdf", Exe },
        { "pdf", Zip },
        { "docx", Pdf },
        { "doc", Zip },
        { "png", Jpeg },
        { "jpg", Png },
        { "webp", "RIFF\0\0\0\0WAVE"u8.ToArray() },
        { "txt", Exe },
        { "csv", Zip },
        { "exe", Exe },
        { "pdf", [] }
    };

    [Theory]
    [MemberData(nameof(MismatchedFiles))]
    public async Task MatchesExtensionAsync_WhenBytesDisagreeWithExtension_ReturnsFalse(string extension, byte[] content)
    {
        var matches = await Service().MatchesExtensionAsync(File(content, $"file.{extension}"), extension, CancellationToken.None);

        Assert.False(matches);
    }

    [Theory]
    [InlineData("pdf", true)]
    [InlineData("PDF", true)]
    [InlineData("docx", true)]
    [InlineData("zip", true)]
    [InlineData("exe", false)]
    [InlineData("svg", false)]
    [InlineData("html", false)]
    [InlineData("rar", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsAllowedExtension_WithDefaultList_AcceptsOnlyListedTypes(string? extension, bool allowed)
    {
        Assert.Equal(allowed, Service().IsAllowedExtension(extension));
    }

    [Fact]
    public void IsAllowedExtension_WhenConfiguredListNamesAnUnknownType_StillRejectsIt()
    {
        var service = Service("pdf", "exe");

        Assert.True(service.IsAllowedExtension("pdf"));
        Assert.False(service.IsAllowedExtension("exe"));
        Assert.False(service.IsAllowedExtension("docx"));
    }

    [Theory]
    [InlineData("pdf", "application/pdf")]
    [InlineData("docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
    [InlineData("JPG", "image/jpeg")]
    [InlineData("csv", "text/csv")]
    [InlineData("unknown", "application/octet-stream")]
    public void GetContentType_ReturnsTypeOfTheExtension(string extension, string contentType)
    {
        Assert.Equal(contentType, Service().GetContentType(extension));
    }

    [Theory]
    [InlineData("application/pdf", true)]
    [InlineData("image/png", true)]
    [InlineData("image/webp", true)]
    [InlineData("application/zip", false)]
    [InlineData("text/plain", true)]
    [InlineData("text/csv", true)]
    [InlineData("application/msword", false)]
    public void CanPreview_AllowsImagesPdfAndText(string contentType, bool canPreview)
    {
        Assert.Equal(canPreview, Service().CanPreview(contentType));
    }
}

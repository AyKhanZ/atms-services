using ATMS.Application.Exceptions.Configuration;
using ATMS.Infrastructure.Files;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Project.Services.Tests.Infrastructure.Files;

public sealed class LocalFileStorageTest : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"atms-attachments-{Guid.NewGuid():N}");

    private LocalFileStorage Storage() =>
        new(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["AttachmentsOptions:RootPath"] = _root })
            .Build());

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    [Fact]
    public async Task SaveAsync_WritesTheExactBytesUnderAGeneratedName()
    {
        var content = new byte[] { 1, 2, 3, 4, 5 };
        var file = new FormFile(new MemoryStream(content), 0, content.Length, "file", "Отчёт.PDF");

        var relativePath = await Storage().SaveAsync(file, "project/2026/09", "PDF", CancellationToken.None);

        Assert.StartsWith("project/2026/09/", relativePath);
        Assert.EndsWith(".pdf", relativePath);
        Assert.DoesNotContain("Отчёт", relativePath);
        Assert.Equal(content, await File.ReadAllBytesAsync(Path.Combine(_root, relativePath)));
        Assert.Empty(Directory.GetFiles(Path.Combine(_root, "project", "2026", "09"), "*.tmp"));
    }

    [Fact]
    public async Task DeleteAsync_RemovesTheFile()
    {
        var file = new FormFile(new MemoryStream([1]), 0, 1, "file", "a.txt");
        var storage = Storage();
        var relativePath = await storage.SaveAsync(file, "project", "txt", CancellationToken.None);

        await storage.DeleteAsync(relativePath, CancellationToken.None);

        Assert.False(File.Exists(storage.GetFullPath(relativePath)));
    }

    [Fact]
    public void Constructor_WhenSectionIsMissing_ThrowsConfigurationException()
    {
        var exception = Assert.Throws<ConfigurationException>(
            () => new LocalFileStorage(new ConfigurationBuilder().Build()));

        Assert.Equal(ConfigurationErrorType.AttachmentsSectionNotFound, exception.ErrorType);
    }

    [Theory]
    [InlineData("../outside.txt")]
    [InlineData("project/../../outside.txt")]
    public void GetFullPath_WhenPathLeavesTheRoot_Throws(string relativePath)
    {
        Assert.Throws<InvalidOperationException>(() => Storage().GetFullPath(relativePath));
    }
}

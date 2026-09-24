using ATMS.Project.API.Results;
using ATMS.Project.Contracts.Models.Attachments;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Project.API.Tests;

public sealed class AttachmentContentResultTest : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.pdf");
    private readonly Guid _attachmentId = Guid.NewGuid();

    public AttachmentContentResultTest()
    {
        File.WriteAllBytes(_path, "%PDF-1.7"u8.ToArray());
    }

    public void Dispose()
    {
        File.Delete(_path);
    }

    private async Task<HttpResponse> Execute(bool canPreview, bool inline)
    {
        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IActionResultExecutor<PhysicalFileResult>, PhysicalFileResultExecutor>()
            .BuildServiceProvider();
        var httpContext = new DefaultHttpContext { RequestServices = services };
        httpContext.Response.Body = new MemoryStream();
        var content = new AttachmentContentModel
        {
            FileName = "Отчёт.pdf",
            ContentType = "application/pdf",
            PhysicalPath = _path,
            CanPreview = canPreview
        };

        await new AttachmentContentResult(content, _attachmentId, inline)
            .ExecuteResultAsync(new ActionContext(httpContext, new RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()));

        return httpContext.Response;
    }

    [Fact]
    public async Task ByDefault_SendsADownloadUnderTheOriginalName()
    {
        var response = await Execute(canPreview: true, inline: false);

        Assert.StartsWith("attachment", response.Headers.ContentDisposition.ToString());
        Assert.Contains("filename*=UTF-8''%D0%9E%D1%82%D1%87%D1%91%D1%82.pdf", response.Headers.ContentDisposition.ToString());
        Assert.Equal("application/pdf", response.ContentType);
    }

    [Fact]
    public async Task Inline_ForAPreviewableFile_ShowsItInTheBrowser()
    {
        var response = await Execute(canPreview: true, inline: true);

        Assert.StartsWith("inline", response.Headers.ContentDisposition.ToString());
    }

    // A browser must never render an archive or an Office file itself, whatever the client asks.
    [Fact]
    public async Task Inline_ForAFileThatCannotBePreviewed_StillDownloads()
    {
        var response = await Execute(canPreview: false, inline: true);

        Assert.StartsWith("attachment", response.Headers.ContentDisposition.ToString());
    }

    // Opened on its own, whatever it really is, the file is a sandboxed document with no scripts,
    // no guessed type and no way to be framed.
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task EveryResponse_IsLockedDownAgainstScriptsSniffingAndFraming(bool inline)
    {
        var response = await Execute(canPreview: true, inline);

        Assert.Equal("nosniff", response.Headers.XContentTypeOptions.ToString());
        var policy = response.Headers.ContentSecurityPolicy.ToString();
        Assert.Contains("default-src 'none'", policy);
        Assert.Contains("sandbox", policy);
        Assert.Contains("frame-ancestors 'none'", policy);
    }

    // The bytes behind an id never change, so opening a file again must not download it again.
    [Fact]
    public async Task EveryResponse_LetsTheBrowserKeepTheFileAndRevalidateItByItsId()
    {
        var response = await Execute(canPreview: true, inline: false);

        Assert.StartsWith("private, max-age=", response.Headers.CacheControl.ToString());
        Assert.Equal($"\"{_attachmentId:N}\"", response.Headers.ETag.ToString());
    }
}

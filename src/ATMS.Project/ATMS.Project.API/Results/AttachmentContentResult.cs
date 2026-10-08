using ATMS.Project.Contracts.Models.Attachments;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace ATMS.Project.API.Results;

public sealed class AttachmentContentResult(AttachmentContentModel content, Guid attachmentId, bool inline)
    : IActionResult
{
    private const int CacheSeconds = 7 * 24 * 60 * 60;

    public Task ExecuteResultAsync(ActionContext context)
    {
        var headers = context.HttpContext.Response.Headers;

        // security: the browser must not sniff, run or frame the file
        headers.XContentTypeOptions = "nosniff";
        headers.ContentSecurityPolicy = "default-src 'none'; sandbox; frame-ancestors 'none'";

        // the file under an id never changes (rename only touches the db), so cache it for a week
        headers.CacheControl = $"private, max-age={CacheSeconds}";
        var entityTag = new EntityTagHeaderValue($"\"{attachmentId:N}\"");

        // inline only for safe types (images, pdf, text), everything else is a download
        var shown = inline && content.CanPreview;
        if (shown)
        {
            headers.ContentDisposition = new ContentDispositionHeaderValue("inline")
            {
                FileNameStar = content.FileName
            }.ToString();
        }

        var file = new PhysicalFileResult(content.PhysicalPath, content.ContentType)
        {
            FileDownloadName = shown ? string.Empty : content.FileName,
            EntityTag = entityTag,
            EnableRangeProcessing = true
        };

        return file.ExecuteResultAsync(context);
    }
}

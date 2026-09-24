using ATMS.Project.Contracts.Models.Attachments;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace ATMS.Project.API.Results;

// A stored attachment as an HTTP response: the file streamed from disk with the headers that keep
// it safe to hand to a browser and cheap to open again. Kept out of the controller so the action
// stays a request and a return.
public sealed class AttachmentContentResult(AttachmentContentModel content, Guid attachmentId, bool inline)
    : IActionResult
{
    private const int CacheSeconds = 7 * 24 * 60 * 60;

    public Task ExecuteResultAsync(ActionContext context)
    {
        var headers = context.HttpContext.Response.Headers;

        // Whatever the bytes are, the browser must not guess another type, run them as a page or
        // frame them: a file opened on its own is a sandboxed document with no scripts.
        headers.XContentTypeOptions = "nosniff";
        headers.ContentSecurityPolicy = "default-src 'none'; sandbox; frame-ancestors 'none'";

        // The bytes behind an id never change — a rename touches only the database — so the
        // browser keeps its copy and opening a file again costs no download. Private: the file is
        // not for shared caches. After a week the ETag turns a re-check into an empty 304.
        headers.CacheControl = $"private, max-age={CacheSeconds}";
        var entityTag = new EntityTagHeaderValue($"\"{attachmentId:N}\"");

        // Shown in the browser only when asked and only for the kinds it can show harmlessly
        // (images, PDF, text); everything else is always a download.
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

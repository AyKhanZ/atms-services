namespace ATMS.Infrastructure.Options;

public class AttachmentsOptions
{
    public required string RootPath { get; init; }
    public long MaxFileSizeBytes { get; init; } = 25 * 1024 * 1024;
    public int MaxFilesPerOwner { get; init; } = 100;

    // Null until configured: the binder appends a configured array to an initialized one instead of
    // replacing it, so a default here could never be narrowed from appsettings.
    public string[]? AllowedExtensions { get; init; }

    public static readonly string[] DefaultAllowedExtensions =
    [
        "pdf",
        "doc", "docx",
        "xls", "xlsx",
        "ppt", "pptx",
        "odt", "ods", "odp",
        "jpg", "jpeg", "png", "webp", "gif",
        "txt", "csv",
        "zip"
    ];
}

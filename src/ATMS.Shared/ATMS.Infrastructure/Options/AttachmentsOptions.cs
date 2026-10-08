using System.ComponentModel.DataAnnotations;

namespace ATMS.Infrastructure.Options;

public sealed class AttachmentsOptions
{
    [Required]
    public required string RootPath { get; init; }

    public long MaxFileSizeBytes { get; init; } = 25 * 1024 * 1024;
    public int MaxFilesPerOwner { get; init; } = 100;

    // no default: the binder appends to an initialized array instead of replacing it
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

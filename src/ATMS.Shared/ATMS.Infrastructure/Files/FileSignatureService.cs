using ATMS.Infrastructure.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace ATMS.Infrastructure.Files;

// The browser's content type is written by the client, so a file is trusted only when its first
// bytes agree with its extension. Office Open XML, OpenDocument and ZIP share one signature: telling
// them apart would mean opening the archive, and a renamed ZIP is still harmless on download.
public sealed class FileSignatureService(IConfiguration configuration) : IFileSignatureService
{
    private const int TextProbeLength = 8 * 1024;

    private static readonly byte[] Pdf = "%PDF"u8.ToArray();
    private static readonly byte[] Zip = [0x50, 0x4B, 0x03, 0x04];
    private static readonly byte[] Ole = [0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1];
    private static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF];
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static readonly byte[] Gif = "GIF8"u8.ToArray();
    private static readonly byte[] Riff = "RIFF"u8.ToArray();
    private static readonly byte[] Webp = "WEBP"u8.ToArray();

    private static readonly IReadOnlyDictionary<string, FileKind> Kinds = new Dictionary<string, FileKind>
    {
        ["pdf"] = new("application/pdf", Signature.Pdf),
        ["doc"] = new("application/msword", Signature.Ole),
        ["xls"] = new("application/vnd.ms-excel", Signature.Ole),
        ["ppt"] = new("application/vnd.ms-powerpoint", Signature.Ole),
        ["docx"] = new("application/vnd.openxmlformats-officedocument.wordprocessingml.document", Signature.Zip),
        ["xlsx"] = new("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", Signature.Zip),
        ["pptx"] = new("application/vnd.openxmlformats-officedocument.presentationml.presentation", Signature.Zip),
        ["odt"] = new("application/vnd.oasis.opendocument.text", Signature.Zip),
        ["ods"] = new("application/vnd.oasis.opendocument.spreadsheet", Signature.Zip),
        ["odp"] = new("application/vnd.oasis.opendocument.presentation", Signature.Zip),
        ["zip"] = new("application/zip", Signature.Zip),
        ["jpg"] = new("image/jpeg", Signature.Jpeg),
        ["jpeg"] = new("image/jpeg", Signature.Jpeg),
        ["png"] = new("image/png", Signature.Png),
        ["gif"] = new("image/gif", Signature.Gif),
        ["webp"] = new("image/webp", Signature.Webp),
        ["txt"] = new("text/plain", Signature.Text),
        ["csv"] = new("text/csv", Signature.Text)
    };

    private static readonly HashSet<string> PreviewContentTypes =
    [
        "application/pdf",
        "image/jpeg",
        "image/png",
        "image/gif",
        "image/webp",
        "text/plain",
        "text/csv"
    ];

    private readonly HashSet<string> _allowedExtensions =
        (configuration.GetSection(nameof(AttachmentsOptions)).Get<AttachmentsOptions>()?.AllowedExtensions
         ?? AttachmentsOptions.DefaultAllowedExtensions)
        .Select(extension => extension.ToLowerInvariant())
        .Where(Kinds.ContainsKey)
        .ToHashSet();

    public bool IsAllowedExtension(string? extension)
    {
        return !string.IsNullOrWhiteSpace(extension) && _allowedExtensions.Contains(extension.ToLowerInvariant());
    }

    public string GetContentType(string extension)
    {
        return Kinds.TryGetValue(extension.ToLowerInvariant(), out var kind)
            ? kind.ContentType
            : "application/octet-stream";
    }

    public bool CanPreview(string contentType)
    {
        return PreviewContentTypes.Contains(contentType);
    }

    public async Task<bool> MatchesExtensionAsync(IFormFile file, string extension, CancellationToken cancellationToken)
    {
        if (!Kinds.TryGetValue(extension.ToLowerInvariant(), out var kind))
        {
            return false;
        }

        var buffer = new byte[kind.Signature == Signature.Text ? TextProbeLength : 12];
        await using var stream = file.OpenReadStream();
        var length = await stream.ReadAtLeastAsync(buffer, buffer.Length, throwOnEndOfStream: false, cancellationToken);
        var head = buffer.AsSpan(0, length);

        return kind.Signature switch
        {
            Signature.Pdf => head.StartsWith(Pdf),
            Signature.Zip => head.StartsWith(Zip),
            Signature.Ole => head.StartsWith(Ole),
            Signature.Jpeg => head.StartsWith(Jpeg),
            Signature.Png => head.StartsWith(Png),
            Signature.Gif => head.StartsWith(Gif),
            Signature.Webp => head.Length >= 12 && head.StartsWith(Riff) && head[8..12].SequenceEqual(Webp),
            Signature.Text => IsText(head),
            _ => false
        };
    }

    // A zero byte never appears in UTF-8 or single-byte text, but is in every executable and
    // archive. UTF-16 is the one text encoding full of zeros, and it announces itself with a BOM.
    private static bool IsText(ReadOnlySpan<byte> head)
    {
        var isUtf16 = head.Length >= 2 &&
                      ((head[0] == 0xFF && head[1] == 0xFE) || (head[0] == 0xFE && head[1] == 0xFF));

        return isUtf16 || !head.Contains((byte)0);
    }

    private enum Signature
    {
        Pdf,
        Zip,
        Ole,
        Jpeg,
        Png,
        Gif,
        Webp,
        Text
    }

    private sealed record FileKind(string ContentType, Signature Signature);
}

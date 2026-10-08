using ATMS.Project.Services.Domain.Attachments.Interfaces;

namespace ATMS.Project.Services.Domain.Attachments;

// only the display name, the file on disk has a generated one; the extension is fixed, so a rename can't make an .exe
public sealed class AttachmentFileNameService : IAttachmentFileNameService
{
    private const string FallbackBaseName = "file";

    private static readonly char[] InvalidCharacters = ['\\', '/', ':', '*', '?', '"', '<', '>', '|'];

    public int MaxBaseNameLength => 200;

    public bool HasInvalidCharacters(string baseName)
    {
        return baseName.IndexOfAny(InvalidCharacters) >= 0 || baseName.Any(char.IsControl);
    }

    public string FromUpload(string uploadedFileName, string extension)
    {
        // old windows browsers send the full path
        var lastSegment = uploadedFileName.Split('/', '\\').Last();
        var dot = lastSegment.LastIndexOf('.');
        var baseName = dot >= 0 ? lastSegment[..dot] : lastSegment;
        var cleaned = new string(baseName
                .Where(character => !char.IsControl(character) && Array.IndexOf(InvalidCharacters, character) < 0)
                .ToArray())
            .Trim()
            .TrimEnd('.');

        if (cleaned.Length == 0)
        {
            cleaned = FallbackBaseName;
        }

        return $"{Truncate(cleaned)}.{extension}";
    }

    public string Rename(string newBaseName, string currentFileName)
    {
        return $"{Truncate(newBaseName.Trim())}{Path.GetExtension(currentFileName)}";
    }

    private string Truncate(string baseName)
    {
        return baseName.Length <= MaxBaseNameLength ? baseName : baseName[..MaxBaseNameLength].TrimEnd();
    }
}

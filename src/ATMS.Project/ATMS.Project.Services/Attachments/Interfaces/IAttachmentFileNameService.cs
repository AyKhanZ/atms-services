namespace ATMS.Project.Services.Attachments.Interfaces;

public interface IAttachmentFileNameService
{
    int MaxBaseNameLength { get; }

    bool HasInvalidCharacters(string baseName);

    string FromUpload(string uploadedFileName, string extension);

    string Rename(string newBaseName, string currentFileName);
}

namespace ATMS.Project.Data.Models.Attachments;

public sealed record AttachmentAuthor(Guid Id, string Name, string Surname, string? AvatarPath);

namespace ATMS.Project.Data.Models.Attachments;

public sealed record AttachmentTicketCount(
    Guid GroupId,
    string GroupTitle,
    DateTime GroupCreatedAt,
    Guid MilestoneId,
    string MilestoneTitle,
    DateTime MilestoneCreatedAt,
    Guid TicketId,
    string TicketCode,
    string TicketTitle,
    DateTime TicketCreatedAt,
    int FileCount);

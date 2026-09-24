namespace ATMS.Project.Data.Models.History;

public sealed record HistoryStatusChange(
    DateTime CreatedAt,
    Guid? CreatedById,
    string? OldValue,
    string? NewValue);

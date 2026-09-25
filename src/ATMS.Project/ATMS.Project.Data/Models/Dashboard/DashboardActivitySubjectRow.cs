namespace ATMS.Project.Data.Models.Dashboard;

public sealed record DashboardActivitySubjectRow(
    Guid Id,
    string Type,
    string Code,
    string Title,
    bool IsDeleted,
    Guid? WorkTicketId);

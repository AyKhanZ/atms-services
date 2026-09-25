using ATMS.Application.Models;

namespace ATMS.Project.Data.Models.Dashboard;

public sealed class DashboardWorkloadRow : AuditUserModel
{
    public string? AvatarPath { get; init; }

    public int Count { get; init; }
}

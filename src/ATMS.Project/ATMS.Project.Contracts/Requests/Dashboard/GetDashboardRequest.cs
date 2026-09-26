using ATMS.Application.Security;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Models.Dashboard;
using MediatR;

namespace ATMS.Project.Contracts.Requests.Dashboard;

/// <summary>Metrics, charts, deadlines and recent activity from projects the caller may view.</summary>
/// <remarks>
/// Omit projectId for all accessible projects. Period is today, 7d, 30d, thisMonth, 6m, 12m or custom;
/// every period ends today in the business time zone. Custom takes from and to (yyyy-MM-dd), up to
/// today and no longer than one year.
/// </remarks>
[Access(PermissionEnum.ProjectView)]
public sealed class GetDashboardRequest : IRequest<DashboardModel>
{
    public Guid? ProjectId { get; init; }

    public string Period { get; init; } = DashboardPeriods.Last30Days;

    public DateOnly? From { get; init; }

    public DateOnly? To { get; init; }
}

using ATMS.Application.Security;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Models.Dashboard;
using MediatR;

namespace ATMS.Project.Contracts.Requests.Dashboard;

/// <summary>Metrics, charts, deadlines and recent activity from projects the caller may view.</summary>
/// <remarks>Omit projectId for all accessible projects. Period is 7, 30 or 90 business days, including today.</remarks>
[Access(PermissionEnum.ProjectView)]
public sealed class GetDashboardRequest : IRequest<DashboardModel>
{
    public Guid? ProjectId { get; init; }

    public int Period { get; init; } = 30;
}

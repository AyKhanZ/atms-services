using ATMS.Data.Criteria.Interfaces;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Models.Dashboard;

namespace ATMS.Project.Data.Repositories.Interfaces;

public interface IDashboardRepository
{
    Task<bool> IsProjectAccessibleAsync(
        ICriteria<WorkProject> accessibleProjects,
        Guid projectId,
        CancellationToken cancellationToken);

    Task<DashboardData> GetAsync(
        ICriteria<WorkProject> accessibleProjects,
        Guid? projectId,
        DateTime todayStartUtc,
        DateTime periodStartUtc,
        DateTime previousStartUtc,
        DateTime periodEndUtc,
        DateTime dueEndUtc,
        double offsetHours,
        bool includeWorkload,
        CancellationToken cancellationToken);
}

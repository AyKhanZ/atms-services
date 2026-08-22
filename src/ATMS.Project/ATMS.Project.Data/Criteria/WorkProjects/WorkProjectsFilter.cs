using ATMS.Data.Criteria;
using ATMS.Data.Enums;
using ATMS.Project.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ATMS.Project.Data.Criteria.WorkProjects;

public class WorkProjectsFilter : ACriteria<WorkProject>
{
    public string? Search { get; init; }

    public DateTime? StartDate { get; init; }

    public DateTime? EndDate { get; init; }

    public int? ProjectTypeId { get; init; }

    public int? ProjectKindId { get; init; }

    public int? ProjectStatusId { get; init; }

    public string? SortBy { get; init; }

    public SortDirectionEnum SortDirection { get; init; } = SortDirectionEnum.Asc;

    public override IQueryable<WorkProject> Apply(IQueryable<WorkProject> query)
    {
        if (!string.IsNullOrWhiteSpace(Search))
        {
            var search = Search.Trim();
            query = query.Where(x =>
                EF.Functions.ILike(x.Title, $"%{search}%") ||
                EF.Functions.ILike(x.Code, $"%{search}%") ||
                (x.Organization != null && EF.Functions.ILike(x.Organization.Title, $"%{search}%")));
        }

        if (StartDate.HasValue)
        {
            var startDate = DateTime.SpecifyKind(StartDate.Value, DateTimeKind.Utc);
            query = query.Where(x => x.StartDate >= startDate);
        }

        if (EndDate.HasValue)
        {
            var endDate = DateTime.SpecifyKind(EndDate.Value, DateTimeKind.Utc);
            query = query.Where(x => x.EndDate <= endDate);
        }

        if (ProjectTypeId.HasValue)
        {
            query = query.Where(x => x.ProjectTypeId == ProjectTypeId);
        }

        if (ProjectKindId.HasValue)
        {
            query = query.Where(x => x.ProjectKindId == ProjectKindId);
        }

        if (ProjectStatusId.HasValue)
        {
            query = query.Where(x => x.ProjectStatusId == ProjectStatusId);
        }

        return ApplySorting(query);
    }

    private IQueryable<WorkProject> ApplySorting(IQueryable<WorkProject> query)
    {
        var ascending = SortDirection == SortDirectionEnum.Asc;

        return SortBy?.ToLowerInvariant() switch
        {
            "code" => ascending ? query.OrderBy(x => x.Code) : query.OrderByDescending(x => x.Code),
            "title" => ascending ? query.OrderBy(x => x.Title) : query.OrderByDescending(x => x.Title),
            "organization" => ascending
                ? query.OrderBy(x => x.Organization == null ? null : x.Organization.Title)
                : query.OrderByDescending(x => x.Organization == null ? null : x.Organization.Title),
            "startdate" => ascending ? query.OrderBy(x => x.StartDate) : query.OrderByDescending(x => x.StartDate),
            "enddate" => ascending ? query.OrderBy(x => x.EndDate) : query.OrderByDescending(x => x.EndDate),
            "projecttype" => ascending
                ? query.OrderBy(x => x.ProjectType.Code) : query.OrderByDescending(x => x.ProjectType.Code),
            "projectkind" => ascending
                ? query.OrderBy(x => x.ProjectKind.Code) : query.OrderByDescending(x => x.ProjectKind.Code),
            "projectstatus" => ascending
                ? query.OrderBy(x => x.ProjectStatus.Code) : query.OrderByDescending(x => x.ProjectStatus.Code),
            "createdat" => ascending ? query.OrderBy(x => x.CreatedAt) : query.OrderByDescending(x => x.CreatedAt),
            _ => query.OrderByDescending(x => x.CreatedAt)
        };
    }
}

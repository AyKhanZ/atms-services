using ATMS.Contracts.Requests;
using ATMS.Data.Criteria;
using ATMS.Project.Contracts.Models.WorkProjects;
using MediatR;

namespace ATMS.Project.Contracts.Requests.WorkProjects;

public class GetWorkProjectsRequest : GetPaginationRequest, IRequest<PagedResult<WorkProjectItemModel>>
{
    /// <summary>Search by project title, code or organization name.</summary>
    public string? Search { get; init; }

    /// <summary>Filter projects starting on or after this date.</summary>
    public DateTime? StartDate { get; init; }

    /// <summary>Filter projects ending on or before this date.</summary>
    public DateTime? EndDate { get; init; }

    /// <summary>Filter by project type.</summary>
    public int? ProjectTypeId { get; init; }

    /// <summary>Filter by project kind.</summary>
    public int? ProjectKindId { get; init; }

    /// <summary>Filter by project status.</summary>
    public int? ProjectStatusId { get; init; }
}

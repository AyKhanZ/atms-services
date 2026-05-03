using ATMS.Contracts.Requests;
using ATMS.Data.Criterias;
using ATMS.Project.Contracts.Models.Organization;
using MediatR;

namespace ATMS.Project.Contracts.Requests.Organizations;

public class GetOrganizationsRequest : GetPaginationRequest, IRequest<PagedResult<OrganizationItemModel>>
{
    /// <summary>Filter by title (starts with, case-insensitive)</summary>
    public string? Title { get; init; }

    /// <summary>Filter by voen (starts with, case-insensitive)</summary>
    public string? Voen { get; init; }
    
    /// <summary>Filter organizations created from this date (inclusive)</summary>
    public DateTime? CreatedFrom { get; init; }

    /// <summary>Filter organizations created to this date (inclusive)</summary>
    public DateTime? CreatedTo { get; init; }
}
using ATMS.Contracts.Requests;
using ATMS.Data.Criteria;
using ATMS.Project.Contracts.Models.Organizations;
using MediatR;
using ATMS.Application.Security;
using ATMS.Data.Enums;

namespace ATMS.Project.Contracts.Requests.Organizations;

[Access(PermissionEnum.OrganizationView)]
public class GetOrganizationsRequest : GetPaginationRequest, IRequest<PagedResult<OrganizationItemModel>>
{
    /// <summary>Search by title or voen (starts with, case-insensitive)</summary>
    public string? Search { get; init; }

    /// <summary>Filter organizations created from this date (inclusive)</summary>
    public DateTime? CreatedFrom { get; init; }

    /// <summary>Filter organizations created to this date (inclusive)</summary>
    public DateTime? CreatedTo { get; init; }
}

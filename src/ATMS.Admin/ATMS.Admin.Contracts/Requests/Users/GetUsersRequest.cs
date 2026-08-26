using ATMS.Admin.Contracts.Models.Users;
using ATMS.Contracts.Requests;
using ATMS.Data.Criteria;
using MediatR;
using ATMS.Application.Security;
using ATMS.Data.Enums;

namespace ATMS.Admin.Contracts.Requests.Users;

[Access(PermissionEnum.UserView)]
public class GetUsersRequest : GetPaginationRequest, IRequest<PagedResult<UserListItemModel>>
{
    /// <summary>
    /// Search text for filtering by name, surname, email or position.
    /// </summary>
    public string? Search { get; set; }

    /// <summary>Filter by user status ID (Active = 1, Inactive = 2, Locked = 3)</summary>
    public int? UserStatusId { get; init; }

    /// <summary>Filter users created from this date (inclusive)</summary>
    public DateTime? CreatedFrom { get; init; }

    /// <summary>Filter users created to this date (inclusive)</summary>
    public DateTime? CreatedTo { get; init; }
}

using ATMS.Admin.Contracts.Models.Users;
using ATMS.Contracts.Requests;
using ATMS.Data.Criterias;
using MediatR;

namespace ATMS.Admin.Contracts.Requests.Users;

public class GetUsersRequest : GetPaginationRequest, IRequest<PagedResult<UserListItemModel>>
{
    /// <summary>Filter by first name (starts with, case-insensitive)</summary>
    public string? Name { get; init; }

    /// <summary>Filter by surname (starts with, case-insensitive)</summary>
    public string? Surname { get; init; }

    /// <summary>Filter by email (starts with, case-insensitive)</summary>
    public string? Email { get; init; }

    /// <summary>Filter by user status ID (Active = 1, Inactive = 2, Locked = 3)</summary>
    public int? UserStatusId { get; init; }

    /// <summary>Filter users created from this date (inclusive)</summary>
    public DateTime? CreatedFrom { get; init; }

    /// <summary>Filter users created to this date (inclusive)</summary>
    public DateTime? CreatedTo { get; init; }
}
using ATMS.Admin.Contracts.Models.Users;
using ATMS.Contracts.Requests;
using ATMS.Data.Criterias;
using MediatR;

namespace ATMS.Admin.Contracts.Requests.Users;

public class GetUsersRequest : GetPaginationRequest, IRequest<PagedResult<UserListItemModel>>
{
    public string? Name { get; init; }
    public string? Surname { get; init; }
    public string? Email { get; init; }
    public int? UserStatusId { get; init; }
}
using ATMS.Admin.Contracts.Models.Users;
using MediatR;

namespace ATMS.Admin.Contracts.Requests.Users;

public class GetUsersRequest: IRequest<UserListItemModel[]>;

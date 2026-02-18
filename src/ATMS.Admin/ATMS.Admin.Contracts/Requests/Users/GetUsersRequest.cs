using ATMS.Admin.Contracts.Models;
using MediatR;

namespace ATMS.Admin.Contracts.Requests.Users;

public class GetUsersRequest: IRequest<UserModel[]>
{
}

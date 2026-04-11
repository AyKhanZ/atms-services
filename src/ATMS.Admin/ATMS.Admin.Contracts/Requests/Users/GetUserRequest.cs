using ATMS.Admin.Contracts.Models.Users;
using MediatR;

namespace ATMS.Admin.Contracts.Requests.Users;

public class GetUserRequest :  IRequest<UserModel>
{
    public Guid Id { get; set; }
}

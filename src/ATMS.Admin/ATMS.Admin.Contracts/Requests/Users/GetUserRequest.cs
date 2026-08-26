using ATMS.Admin.Contracts.Models.Users;
using MediatR;
using ATMS.Application.Security;
using ATMS.Data.Enums;

namespace ATMS.Admin.Contracts.Requests.Users;

[Access(PermissionEnum.UserView)]
public class GetUserRequest : IRequest<UserModel>
{
    public Guid Id { get; set; }
}

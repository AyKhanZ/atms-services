using ATMS.Admin.Contracts.Models.Users;
using ATMS.Data.Enums;
using MediatR;
using ATMS.Application.Security;

namespace ATMS.Admin.Contracts.Commands.Account;

[Access(PermissionEnum.UserEdit)]
public class RegisterCommand: IRequest<UserModel>
{
    public required string Name { get; init; }
    
    public required string Surname { get; init; }
    
    public required string Email { get; init; }
    
    public required Guid RoleId { get; init; }
    
    public Guid? OrganizationId { get; init; }
}

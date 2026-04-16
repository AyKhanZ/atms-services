using ATMS.Admin.Contracts.Models.Users;
using ATMS.Data.Enums;
using MediatR;

namespace ATMS.Admin.Contracts.Commands.Account;

public class RegisterCommand: IRequest<UserModel>
{
    public required string Name { get; init; }
    
    public required string Surname { get; init; }
    
    public required string Email { get; init; }
    
    public required UserTypeEnum UserTypeId { get; init; }
    
    public Guid? OrganizationId { get; init; }
}

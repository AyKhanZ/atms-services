using ATMS.Admin.Contracts.Models;
using MediatR;

namespace ATMS.Admin.Contracts.Commands.Account;

public class RegisterCommand: IRequest<UserModel>
{
    public required string Name { get; set; }
    public required string Surname { get; set; }
    public required string Email { get; set; }
    public required Guid RoleId { get; set; }
}

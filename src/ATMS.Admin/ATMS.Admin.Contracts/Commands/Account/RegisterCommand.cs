using ATMS.Admin.Contracts.Models;
using MediatR;

namespace ATMS.Admin.Contracts.Commands.Account;

public class RegisterCommand: IRequest<UserModel>
{
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Email { get; set; }
    public Guid RoleId { get; set; }
}

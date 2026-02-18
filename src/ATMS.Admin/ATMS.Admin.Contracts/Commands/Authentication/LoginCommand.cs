using ATMS.Admin.Contracts.Models;
using MediatR;

namespace ATMS.Admin.Contracts.Commands.Authentication;

public class LoginCommand: IRequest<AccessInfoModel>
{
    public string Email { get; set; }
    public string Password { get; set; }
}

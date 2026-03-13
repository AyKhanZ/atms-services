using ATMS.Admin.Contracts.Models;
using MediatR;

namespace ATMS.Admin.Contracts.Commands.Authentication;

public class LoginCommand: IRequest<AccessInfoModel>
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}

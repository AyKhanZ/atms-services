using MediatR;

namespace ATMS.Admin.Contracts.Commands.Account;

public class ChangePasswordCommand : IRequest
{
    public required string Email { get; set; }
    public required string OldPassword { get; set; }
    public required string NewPassword { get; set; }
}

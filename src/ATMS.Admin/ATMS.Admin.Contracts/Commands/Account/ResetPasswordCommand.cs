using MediatR;

namespace ATMS.Admin.Contracts.Commands.Account;

public sealed class ResetPasswordCommand : IRequest
{
    public required string Password { get; set; }
    public required string ConfirmPassword { get; set; }
    public required string Token { get; set; }
}

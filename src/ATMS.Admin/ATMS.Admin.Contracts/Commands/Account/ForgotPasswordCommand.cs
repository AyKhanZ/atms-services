using MediatR;

namespace ATMS.Admin.Contracts.Commands.Account;

public sealed class ForgotPasswordCommand : IRequest
{
    public required string Email { get; set; }
}

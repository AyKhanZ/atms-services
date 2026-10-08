using MediatR;

namespace ATMS.Admin.Contracts.Commands.Account;

public sealed class ResendEmailConfirmationCommand : IRequest
{
    public required string Email { get; init; }
}

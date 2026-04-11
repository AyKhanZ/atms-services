using MediatR;

namespace ATMS.Admin.Contracts.Commands.Account;

public class ResendEmailConfirmationCommand : IRequest
{
    public required string Email { get; init; }
}

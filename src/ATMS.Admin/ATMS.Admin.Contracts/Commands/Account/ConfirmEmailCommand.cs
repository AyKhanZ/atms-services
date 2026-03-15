using MediatR;

namespace ATMS.Admin.Contracts.Commands.Account;

public class ConfirmEmailCommand : IRequest<bool>
{
    public required string Token { get; init; }
}
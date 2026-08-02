using ATMS.Admin.Contracts.Enums;
using MediatR;

namespace ATMS.Admin.Contracts.Commands.Account;

public class ConfirmEmailCommand : IRequest<ConfirmEmailResultEnum>
{
    public required string Token { get; init; }
}

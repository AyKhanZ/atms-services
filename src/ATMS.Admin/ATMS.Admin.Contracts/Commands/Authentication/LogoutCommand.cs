using MediatR;

namespace ATMS.Admin.Contracts.Commands.Authentication;

public sealed class LogoutCommand : IRequest
{
    public required string RefreshToken { get; init; }
}

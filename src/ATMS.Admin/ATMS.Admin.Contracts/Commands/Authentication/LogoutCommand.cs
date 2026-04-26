using MediatR;

namespace ATMS.Admin.Contracts.Commands.Authentication;

public class LogoutCommand : IRequest
{
    public required string RefreshToken { get; init; }
}

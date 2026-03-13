using ATMS.Admin.Contracts.Models;
using MediatR;

namespace ATMS.Admin.Contracts.Commands.Authentication;

public class RefreshTokenCommand : IRequest<AccessInfoModel>
{
    public required string RefreshToken { get; init; }
}

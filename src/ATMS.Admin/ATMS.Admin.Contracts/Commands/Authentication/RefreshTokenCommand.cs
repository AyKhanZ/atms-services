using ATMS.Admin.Contracts.Models;
using MediatR;

namespace ATMS.Admin.Contracts.Commands.Authentication;

public sealed class RefreshTokenCommand : IRequest<AccessInfoModel>
{
    public required string RefreshToken { get; init; }
}
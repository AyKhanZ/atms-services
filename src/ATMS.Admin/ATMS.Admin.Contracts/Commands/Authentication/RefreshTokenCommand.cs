using ATMS.Admin.Contracts.Models;
using MediatR;

namespace ATMS.Admin.Contracts.Commands.Authentication;

public class RefreshTokenCommand : IRequest<AccessInfoModel>
{
    public string RefreshToken { get; init; }
}

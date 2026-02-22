using ATMS.Admin.Contracts.Commands.Authentication;
using ATMS.Admin.Contracts.Models;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Authentication;

public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, AccessInfoModel>
{
    public Task<AccessInfoModel> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

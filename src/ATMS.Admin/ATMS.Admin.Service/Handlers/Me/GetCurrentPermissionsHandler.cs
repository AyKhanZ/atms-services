using ATMS.Admin.Contracts.Requests.Me;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Me;

public class GetCurrentPermissionsHandler : IRequestHandler<GetCurrentPermissionsRequest, string[]>
{
    public Task<string[]> Handle(GetCurrentPermissionsRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
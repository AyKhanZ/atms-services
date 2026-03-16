using ATMS.Admin.Contracts.Requests.Me;
using ATMS.Application.Models;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Me;

public class GetCurrentRolesHandler : IRequestHandler<GetCurrentRolesRequest, DictionaryModel<Guid>[]>
{
    public Task<DictionaryModel<Guid>[]> Handle(GetCurrentRolesRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
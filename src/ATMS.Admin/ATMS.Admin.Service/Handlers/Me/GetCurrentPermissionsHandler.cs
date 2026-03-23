using ATMS.Admin.Contracts.Requests.Me;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Exceptions.Entity;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Me;

public class GetCurrentPermissionsHandler(
    IUserRepository userRepository) : IRequestHandler<GetCurrentPermissionsRequest, string[]>
{
    public async Task<string[]> Handle(GetCurrentPermissionsRequest request, CancellationToken cancellationToken)
    {
        var isExist = await userRepository.IsExistAsync(r => r.Id == request.UserId, cancellationToken);
        if (!isExist)
        {
            throw new EntityException(EntityErrorType.NotFound, "User not found .");
        }
        
        var permissions = await userRepository.GetPermissionsAsync(request.UserId, cancellationToken);
        
        return permissions.Select(p => p.Code).ToArray();
    }
}

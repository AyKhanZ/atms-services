using ATMS.Admin.Contracts.Requests.Me;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Application.Exceptions.Auth;
using ATMS.Application.Exceptions.Resources;
using ATMS.Application.Interfaces;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Me;

public class GetCurrentPermissionsHandler(
    IUserRepository userRepository,
    ICurrentUser currentUser) : IRequestHandler<GetCurrentPermissionsRequest, string[]>
{
    public async Task<string[]> Handle(GetCurrentPermissionsRequest request, CancellationToken cancellationToken)
    {
        var isExist = await userRepository.IsExistAsync(r => r.Id == currentUser.Id, cancellationToken);
        if (!isExist)
        {
            throw new AuthException(AuthErrorType.InvalidCredentials, ExceptionMessages.InvalidCredentials);
        }
        
        var permissions = await userRepository.GetPermissionsAsync(currentUser.Id, cancellationToken);
        
        return permissions.Select(p => p.Code).ToArray();
    }
}

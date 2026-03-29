using ATMS.Admin.Contracts.Requests.Me;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Application.Exceptions.Auth;
using ATMS.Application.Models;
using ATMS.Application.Exceptions.Resources;
using ATMS.Application.Interfaces;
using AutoMapper;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Me;

public class GetCurrentRolesHandler(
    IUserRepository userRepository,
    ICurrentUser currentUser,
    IMapper mapper) : IRequestHandler<GetCurrentRolesRequest, DictionaryModel<Guid>[]>
{
    public async Task<DictionaryModel<Guid>[]> Handle(GetCurrentRolesRequest request, CancellationToken cancellationToken)
    {
        var isExist = await userRepository.IsExistAsync(r => r.Id == currentUser.Id, cancellationToken);
        if (!isExist)
        {
            throw new AuthException(AuthErrorType.InvalidCredentials, ExceptionMessages.InvalidCredentials);
        }
        
        var roles = await userRepository.GetRolesAsync(currentUser.Id, cancellationToken);
        
        return mapper.Map<DictionaryModel<Guid>[]>(roles);
    }
}

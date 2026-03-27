using ATMS.Admin.Contracts.Requests.Me;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Resources;
using ATMS.Application.Models;
using ATMS.Application.Exceptions.Entity;
using AutoMapper;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Me;

public class GetCurrentRolesHandler(
    IUserRepository userRepository,
    IMapper mapper) : IRequestHandler<GetCurrentRolesRequest, DictionaryModel<Guid>[]>
{
    public async Task<DictionaryModel<Guid>[]> Handle(GetCurrentRolesRequest request, CancellationToken cancellationToken)
    {
        var isExist = await userRepository.IsExistAsync(r => r.Id == request.UserId, cancellationToken);
        if (!isExist)
        {
            throw new EntityException(EntityErrorType.NotFound, AccountMessages.UserNotFound);
        }
        
        var roles = await userRepository.GetRolesAsync(request.UserId, cancellationToken);
        
        return mapper.Map<DictionaryModel<Guid>[]>(roles);
    }
}

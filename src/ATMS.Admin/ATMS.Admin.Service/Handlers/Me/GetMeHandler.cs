using ATMS.Admin.Contracts.Models.Me;
using ATMS.Admin.Contracts.Requests.Me;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Application.Exceptions.Auth;
using ATMS.Application.Exceptions.Resources;
using ATMS.Application.Interfaces;
using AutoMapper;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Me;

public class GetMeHandler(
    IUserRepository userRepository,
    ICurrentUser currentUser,
    IMapper mapper)
    : IRequestHandler<GetMeRequest, MeModel>
{
    public async Task<MeModel> Handle(GetMeRequest request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetMeAsync(currentUser.Id, cancellationToken);

        if (user is null)
        {
            throw new AuthException(AuthErrorType.InvalidCredentials, ExceptionMessages.InvalidCredentials);
        }
        
        return mapper.Map<MeModel>(user);
    }
}

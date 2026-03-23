using ATMS.Admin.Contracts.Models;
using ATMS.Admin.Contracts.Requests.Users;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Exceptions.Entity;
using AutoMapper;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Users;

public class GetUserHandler(
    IUserRepository userRepository,
    IMapper mapper
    ) : IRequestHandler<GetUserRequest, UserModel>
{
    public async Task<UserModel> Handle(GetUserRequest request, CancellationToken cancellationToken)
    {
        var result = await userRepository.GetAsync(request.Id, cancellationToken);

        if (result is null)
        {
            throw new EntityException(EntityErrorType.NotFound, $"User not found .");
        }

        return mapper.Map<UserModel>(result);
    }
}

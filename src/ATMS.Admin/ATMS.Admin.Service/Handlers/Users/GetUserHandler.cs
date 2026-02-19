using ATMS.Admin.Contracts.Models;
using ATMS.Admin.Contracts.Requests.Users;
using ATMS.Admin.Data.Repositories;
using ATMS.Exceptions.Entity;
using AutoMapper;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Users;

public class GetUserHandler(
    UserRepository userRepository,
    Mapper mapper
    ) : IRequestHandler<GetUserRequest, UserModel>
{
    public async Task<UserModel> Handle(GetUserRequest request, CancellationToken cancellationToken)
    {
        var result = await userRepository.GetByIdAsync(request.Id, cancellationToken);

        if (result is null)
        {
            throw new EntityException(EntityErrorType.EntityNotFound, $"User with id {request.Id} not found");
        }

        return mapper.Map<UserModel>(result);
    }
}

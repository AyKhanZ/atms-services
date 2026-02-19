using ATMS.Admin.Contracts.Models;
using ATMS.Admin.Contracts.Requests.Users;
using ATMS.Admin.Data.Repositories;
using AutoMapper;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Users;

public class GetUsersHandler(
    UserRepository userRepository,
    Mapper mapper
    ) : IRequestHandler<GetUsersRequest, UserModel[]>
{
    public async Task<UserModel[]> Handle(GetUsersRequest request, CancellationToken cancellationToken)
    {
        var result = await userRepository.GetAsync();

        return mapper.Map<UserModel[]>(result);
    }
}

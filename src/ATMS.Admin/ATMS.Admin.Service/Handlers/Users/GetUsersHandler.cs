using ATMS.Admin.Contracts.Models;
using ATMS.Admin.Contracts.Requests.Users;
using ATMS.Admin.Data.Interfaces;
using AutoMapper;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Users;

public class GetUsersHandler(
    IUserRepository userRepository,
    IMapper mapper
    ) : IRequestHandler<GetUsersRequest, UserModel[]>
{
    public async Task<UserModel[]> Handle(GetUsersRequest request, CancellationToken cancellationToken)
    {
        var result = await userRepository.GetAsync();

        return mapper.Map<UserModel[]>(result);
    }
}

using ATMS.Admin.Contracts.Models.Users;
using ATMS.Admin.Contracts.Requests.Users;
using ATMS.Admin.Data.Repositories.Interfaces;
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
        var result = await userRepository.GetAsync(cancellationToken);

        return mapper.Map<UserModel[]>(result);
    }
}

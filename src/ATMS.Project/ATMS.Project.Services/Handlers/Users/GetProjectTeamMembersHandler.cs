using ATMS.Data.Enums;
using ATMS.Project.Contracts.Models.Users;
using ATMS.Project.Contracts.Requests.Users;
using ATMS.Project.Data.Repositories.Interfaces;
using AutoMapper;
using MediatR;

namespace ATMS.Project.Services.Handlers.Users;

public class GetProjectTeamMembersHandler(
    IUserRepository userRepository,
    IMapper mapper)
    : IRequestHandler<GetProjectTeamMembersRequest, UserModel[]>
{
    public async Task<UserModel[]> Handle(
        GetProjectTeamMembersRequest request,
        CancellationToken cancellationToken)
    {
        var users = await userRepository.GetManyAsync(
            user => user.UserType != (int)UserTypeEnum.Client,
            cancellationToken);

        return mapper.Map<UserModel[]>(users);
    }
}

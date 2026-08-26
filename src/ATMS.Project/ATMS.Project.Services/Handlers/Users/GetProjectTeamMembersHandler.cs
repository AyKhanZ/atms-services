using ATMS.Data.Criteria.Users;
using ATMS.Project.Data.Criteria.Users;
using ATMS.Project.Contracts.Models.Users;
using ATMS.Project.Contracts.Requests.Users;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using AutoMapper;
using MediatR;

namespace ATMS.Project.Services.Handlers.Users;

public class GetProjectTeamMembersHandler(
    IUserRepository userRepository,
    IMapper mapper) : IRequestHandler<GetProjectTeamMembersRequest, UserModel[]>
{
    public async Task<UserModel[]> Handle(GetProjectTeamMembersRequest request, CancellationToken cancellationToken)
    {
        var criteria = new NotAdminCriteria<User>()
            .And(new NotClientUsersCriteria());

        var users = await userRepository.GetManyAsync(criteria, cancellationToken);

        return mapper.Map<UserModel[]>(users);
    }
}

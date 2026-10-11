using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Exceptions.Enums;
using ATMS.Data.Criteria.Users;
using ATMS.Project.Contracts.Models.Users;
using ATMS.Project.Contracts.Requests.Users;
using ATMS.Project.Data.Criteria.Users;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using AutoMapper;
using MediatR;

namespace ATMS.Project.Services.Handlers.Users;

public sealed class GetProjectClientCandidatesHandler(
    IWorkProjectRepository workProjectRepository,
    IUserRepository userRepository,
    IMapper mapper) : IRequestHandler<GetProjectClientCandidatesRequest, UserModel[]>
{
    public async Task<UserModel[]> Handle(
        GetProjectClientCandidatesRequest request,
        CancellationToken cancellationToken)
    {
        var project = await workProjectRepository.FindRootAsync(request.ProjectId, cancellationToken);
        if (project is null)
        {
            throw new EntityException(EntityErrorTypeEnum.NotFound, WorkProjectMessages.NotFound);
        }

        if (project.OrganizationId is not Guid organizationId)
        {
            return [];
        }

        var criteria = new ClientUsersCriteria()
            .And(new OrganizationUsersCriteria(organizationId))
            .And(new ActiveUsersCriteria());

        var users = await userRepository.GetManyAsync(criteria, cancellationToken);

        return mapper.Map<UserModel[]>(users);
    }
}

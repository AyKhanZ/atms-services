using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Interfaces;
using ATMS.Project.Contracts.Models.WorkProjects;
using ATMS.Project.Contracts.Requests.WorkProjects;
using ATMS.Project.Data.Criteria.WorkProjects;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using AutoMapper;
using MediatR;

namespace ATMS.Project.Services.Handlers.WorkProjects;

public class GetWorkProjectHandler(
    ICurrentUser currentUser,
    IWorkProjectRepository workProjectRepository,
    IMapper mapper)
    : IRequestHandler<GetWorkProjectRequest, WorkProjectModel>
{
    public async Task<WorkProjectModel> Handle(
        GetWorkProjectRequest request,
        CancellationToken cancellationToken)
    {
        var criteria = new AccessibleWorkProjectsCriteria(currentUser.Id, currentUser.RoleId);
        var project = await workProjectRepository.GetAsync(request.Id, criteria, cancellationToken)
            ?? throw new EntityException(EntityErrorType.NotFound, WorkProjectMessages.NotFound);

        return mapper.Map<WorkProjectModel>(project);
    }
}

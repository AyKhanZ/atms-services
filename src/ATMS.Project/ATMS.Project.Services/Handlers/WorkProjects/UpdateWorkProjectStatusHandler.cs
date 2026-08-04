using ATMS.Application.Exceptions.Entity;
using ATMS.Caching.Constants;
using ATMS.Caching.Services.Interfaces;
using ATMS.Project.Contracts.Commands.WorkProjects;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using MediatR;

namespace ATMS.Project.Services.Handlers.WorkProjects;

public class UpdateWorkProjectStatusHandler(
    IWorkProjectRepository workProjectRepository,
    ICacheService cache)
    : IRequestHandler<UpdateWorkProjectStatusCommand>
{
    public async Task Handle(UpdateWorkProjectStatusCommand command, CancellationToken cancellationToken)
    {
        var project = await workProjectRepository.FindAsync(command.Id, cancellationToken);
        if (project is null)
        {
            throw new EntityException(EntityErrorType.NotFound, WorkProjectMessages.NotFound);
        }

        project.ProjectStatusId = command.ProjectStatusId;
        await workProjectRepository.SaveAsync(cancellationToken);
        await cache.RemoveAsync(CacheKeys.Project.ProjectById(project.Id), cancellationToken);
    }
}

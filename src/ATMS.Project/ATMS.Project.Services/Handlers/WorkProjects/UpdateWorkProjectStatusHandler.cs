using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Exceptions.Enums;
using ATMS.Caching.Services.Interfaces;
using ATMS.Project.Contracts.Commands.WorkProjects;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using ATMS.Project.Services.Infrastructure;
using MediatR;

namespace ATMS.Project.Services.Handlers.WorkProjects;

public sealed class UpdateWorkProjectStatusHandler(
    IWorkProjectRepository workProjectRepository,
    ICacheService cache)
    : IRequestHandler<UpdateWorkProjectStatusCommand>
{
    public async Task Handle(UpdateWorkProjectStatusCommand command, CancellationToken cancellationToken)
    {
        var project = await workProjectRepository.FindRootAsync(command.Id, cancellationToken);
        if (project is null)
        {
            throw new EntityException(EntityErrorTypeEnum.NotFound, WorkProjectMessages.NotFound);
        }

        project.ProjectStatusId = command.ProjectStatusId;
        await workProjectRepository.SaveAsync(cancellationToken);
        await cache.RemoveWorkProjectAsync(project.Id, cancellationToken);
    }
}

using ATMS.Application.Exceptions.Entity;
using ATMS.Caching.Services.Interfaces;
using ATMS.Project.Contracts.Commands.WorkTasks;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using ATMS.Project.Services.Caching;
using AutoMapper;
using MediatR;

namespace ATMS.Project.Services.Handlers.WorkTasks;

public class UpdateWorkTaskHandler(
    IMapper mapper,
    IWorkTaskRepository workTaskRepository,
    ICacheService cache) : IRequestHandler<UpdateWorkTaskCommand>
{
    public async Task Handle(UpdateWorkTaskCommand command, CancellationToken cancellationToken)
    {
        var workTask = await workTaskRepository.FindAsync(command.ProjectId, command.WorkTaskId, cancellationToken)
            ?? throw new EntityException(EntityErrorType.NotFound, WorkTaskMessages.NotFound);

        mapper.Map(command, workTask);

        await workTaskRepository.SaveChangesAsync(cancellationToken);
        var childIds = await workTaskRepository.GetChildIdsAsync(workTask.Id, cancellationToken);
        await cache.RemoveWorkTaskAsync(workTask.Id, cancellationToken);
        await cache.RemoveWorkTasksAsync(childIds, cancellationToken);
    }
}

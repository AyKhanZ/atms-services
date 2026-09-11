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

        var previousParentId = workTask.ParentWorkTaskId;
        var parent = command.ParentWorkTaskId.HasValue
            ? await workTaskRepository.FindParentAsync(
                command.ProjectId,
                command.ParentWorkTaskId.Value,
                cancellationToken)
              ?? throw new EntityException(EntityErrorType.NotFound, WorkTaskMessages.ParentNotFound)
            : null;

        mapper.Map(command, workTask);

        // The ticket is derived, never taken from the request when a parent is given: a subtask
        // always lives in its parent's ticket, and letting the client send both invites a mismatch.
        workTask.ParentWorkTaskId = parent?.Id;
        workTask.WorkTicketId = parent?.WorkTicketId ?? command.WorkTicketId;

        // Subtasks carry the parent's ticket, so moving a task has to move them with it —
        // otherwise they stay behind in the old ticket and disappear from both lists.
        var children = await workTaskRepository.FindChildrenAsync(
            command.ProjectId,
            workTask.Id,
            cancellationToken);

        foreach (var child in children)
        {
            child.WorkTicketId = workTask.WorkTicketId;
        }

        await workTaskRepository.SaveChangesAsync(cancellationToken);

        await cache.RemoveWorkTaskAsync(workTask.Id, cancellationToken);
        await cache.RemoveWorkTasksAsync(children.Select(child => child.Id).ToArray(), cancellationToken);

        // Both ends of a re-parent go stale: the old parent loses a subtask, the new one gains it.
        var affectedParents = new[] { previousParentId, workTask.ParentWorkTaskId }
            .Where(id => id.HasValue && id != workTask.Id)
            .Select(id => id!.Value)
            .Distinct()
            .ToArray();

        await cache.RemoveWorkTasksAsync(affectedParents, cancellationToken);
    }
}

using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Exceptions.Enums;
using ATMS.Application.Interfaces;
using ATMS.Caching.Services.Interfaces;
using ATMS.Project.Contracts.Commands.WorkTasks;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using ATMS.Project.Services.Infrastructure;
using MediatR;

namespace ATMS.Project.Services.Handlers.WorkTasks;

public sealed class DeleteWorkTaskHandler(
    ICurrentUser currentUser,
    IWorkTaskRepository workTaskRepository,
    ICacheService cache) : IRequestHandler<DeleteWorkTaskCommand>
{
    public async Task Handle(DeleteWorkTaskCommand command, CancellationToken cancellationToken)
    {
        var workTask = await workTaskRepository.FindAsync(command.ProjectId, command.WorkTaskId, cancellationToken)
            ?? throw new EntityException(EntityErrorTypeEnum.NotFound, WorkTaskMessages.NotFound);

        var subtasks = await workTaskRepository.FindChildrenAsync(command.ProjectId, workTask.Id, cancellationToken);
        var deletedAt = DateTime.UtcNow;

        foreach (var item in subtasks.Prepend(workTask))
        {
            item.IsDeleted = true;
            item.DeletedAt = deletedAt;
            item.DeletedById = currentUser.Id;
        }

        await workTaskRepository.SaveChangesAsync(cancellationToken);
        await cache.RemoveWorkTaskAsync(workTask.Id, cancellationToken);
        await cache.RemoveWorkTasksAsync(subtasks.Select(subtask => subtask.Id).ToArray(), cancellationToken);

        if (workTask.ParentWorkTaskId.HasValue)
        {
            await cache.RemoveWorkTaskAsync(workTask.ParentWorkTaskId.Value, cancellationToken);
        }
    }
}

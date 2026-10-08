using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Exceptions.Enums;
using ATMS.Caching.Services.Interfaces;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Commands.WorkTasks;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using ATMS.Project.Services.Infrastructure;
using ATMS.Project.Services.Domain.Board.Interfaces;
using ATMS.Project.Services.Domain.Notifications.Interfaces;
using AutoMapper;
using MediatR;

namespace ATMS.Project.Services.Handlers.WorkTasks;

public sealed class UpdateWorkTaskHandler(
    IMapper mapper,
    IWorkTaskRepository workTaskRepository,
    ICacheService cache,
    IWorkTaskBoardPlacementService placement,
    IWorkTaskNotificationService notifications) : IRequestHandler<UpdateWorkTaskCommand>
{
    public async Task Handle(UpdateWorkTaskCommand command, CancellationToken cancellationToken)
    {
        var workTask = await workTaskRepository.FindAsync(command.ProjectId, command.WorkTaskId, cancellationToken)
            ?? throw new EntityException(EntityErrorTypeEnum.NotFound, WorkTaskMessages.NotFound);

        var previousParentId = workTask.ParentWorkTaskId;
        var previousAssigneeId = workTask.AssigneeId;
        var previousStatusId = workTask.StatusId;
        var parent = command.ParentWorkTaskId.HasValue
            ? await workTaskRepository.FindParentAsync(
                command.ProjectId,
                command.ParentWorkTaskId.Value,
                cancellationToken)
              ?? throw new EntityException(EntityErrorTypeEnum.NotFound, WorkTaskMessages.ParentNotFound)
            : null;

        mapper.Map(command, workTask);
        var now = DateTime.UtcNow;

        if (workTask.StatusId != command.StatusId)
        {
            workTask.StatusId = command.StatusId;
            workTask.DoneAt = command.StatusId == (int)WorkTaskStatusEnum.Done ? now : null;

            // new column: the card goes on top, same as Move on the board
            if (command.StatusId != (int)WorkTaskStatusEnum.Done)
            {
                await placement.PlaceOnTopAsync(workTask, cancellationToken);
            }
        }

        workTask.ParentWorkTaskId = parent?.Id;
        workTask.WorkTicketId = parent?.WorkTicketId ?? command.WorkTicketId;

        var children = await workTaskRepository.FindChildrenAsync(
            command.ProjectId,
            workTask.Id,
            cancellationToken);

        foreach (var child in children)
        {
            child.WorkTicketId = workTask.WorkTicketId;
        }

        if (command.CompleteSubtasks && workTask.StatusId == (int)WorkTaskStatusEnum.Done)
        {
            foreach (var child in children.Where(child => child.StatusId != (int)WorkTaskStatusEnum.Done))
            {
                child.StatusId = (int)WorkTaskStatusEnum.Done;
                child.DoneAt = now;
            }
        }

        await notifications.NotifyChangedAsync(workTask, previousAssigneeId, previousStatusId, cancellationToken);
        await placement.SaveAsync(workTask, cancellationToken);

        await cache.RemoveWorkTaskAsync(workTask.Id, cancellationToken);
        await cache.RemoveWorkTasksAsync(children.Select(child => child.Id).ToArray(), cancellationToken);

        var affectedParents = new[] { previousParentId, workTask.ParentWorkTaskId }
            .Where(id => id.HasValue && id != workTask.Id)
            .Select(id => id!.Value)
            .Distinct()
            .ToArray();

        await cache.RemoveWorkTasksAsync(affectedParents, cancellationToken);
    }
}

using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Exceptions.Enums;
using ATMS.Caching.Services.Interfaces;
using ATMS.Project.Contracts.Commands.WorkTasks;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Infrastructure;
using ATMS.Project.Services.Resources;
using MediatR;

namespace ATMS.Project.Services.Handlers.WorkTasks;

public sealed class UpdateWorkTaskDeadlineHandler(
    IWorkTaskRepository workTaskRepository,
    ICacheService cache) : IRequestHandler<UpdateWorkTaskDeadlineCommand>
{
    public async Task Handle(UpdateWorkTaskDeadlineCommand command, CancellationToken cancellationToken)
    {
        var workTask = await workTaskRepository.FindAsync(command.ProjectId, command.WorkTaskId, cancellationToken)
            ?? throw new EntityException(EntityErrorTypeEnum.NotFound, WorkTaskMessages.NotFound);

        workTask.Deadline = command.Deadline;

        await workTaskRepository.SaveChangesAsync(cancellationToken);
        await cache.RemoveWorkTaskAsync(workTask.Id, cancellationToken);
    }
}

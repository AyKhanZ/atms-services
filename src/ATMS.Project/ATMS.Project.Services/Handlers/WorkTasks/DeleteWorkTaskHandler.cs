using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Interfaces;
using ATMS.Caching.Services.Interfaces;
using ATMS.Project.Contracts.Commands.WorkTasks;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using ATMS.Project.Services.Caching;
using MediatR;

namespace ATMS.Project.Services.Handlers.WorkTasks;

public class DeleteWorkTaskHandler(
    ICurrentUser currentUser,
    IWorkTaskRepository workTaskRepository,
    ICacheService cache) : IRequestHandler<DeleteWorkTaskCommand>
{
    public async Task Handle(DeleteWorkTaskCommand command, CancellationToken cancellationToken)
    {
        var workTask = await workTaskRepository.FindAsync(command.ProjectId, command.WorkTaskId, cancellationToken)
            ?? throw new EntityException(EntityErrorType.NotFound, WorkTaskMessages.NotFound);

        workTask.IsDeleted = true;
        workTask.DeletedAt = DateTime.UtcNow;
        workTask.DeletedById = currentUser.Id;

        await workTaskRepository.SaveChangesAsync(cancellationToken);
        await cache.RemoveWorkTaskAsync(workTask.Id, cancellationToken);
    }
}

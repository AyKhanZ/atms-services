using ATMS.Application.Exceptions.Entity;
using ATMS.Caching.Services.Interfaces;
using ATMS.Project.Contracts.Commands.WorkGroups;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Caching;
using ATMS.Project.Services.Resources;
using MediatR;

namespace ATMS.Project.Services.Handlers.WorkGroups;

public class UpdateWorkGroupHandler(
    IWorkGroupRepository workGroupRepository,
    IWorkTicketRepository workTicketRepository,
    IWorkTaskRepository workTaskRepository,
    ICacheService cache) : IRequestHandler<UpdateWorkGroupCommand>
{
    public async Task Handle(UpdateWorkGroupCommand command, CancellationToken cancellationToken)
    {
        var workGroup = await workGroupRepository.FindAsync(command.ProjectId, command.WorkGroupId, cancellationToken);
        if (workGroup is null)
        {
            throw new EntityException(EntityErrorType.NotFound, WorkGroupMessages.NotFound);
        }

        workGroup.Title = command.Title.Trim();
        await workGroupRepository.SaveChangesAsync(cancellationToken);

        var workTicketIds = await workTicketRepository.GetIdsByWorkGroupAsync(command.ProjectId, workGroup.Id, cancellationToken);
        var workTaskIds = await workTaskRepository.GetIdsByTicketsAsync(workTicketIds, cancellationToken);

        await cache.RemoveWorkTicketsAsync(workTicketIds, cancellationToken);
        await cache.RemoveWorkTasksAsync(workTaskIds, cancellationToken);
    }
}

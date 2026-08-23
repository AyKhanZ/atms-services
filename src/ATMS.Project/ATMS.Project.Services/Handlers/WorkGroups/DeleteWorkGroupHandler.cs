using ATMS.Application.Exceptions.Conflict;
using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Interfaces;
using ATMS.Project.Contracts.Commands.WorkGroups;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using MediatR;

namespace ATMS.Project.Services.Handlers.WorkGroups;

public class DeleteWorkGroupHandler(
    ICurrentUser currentUser,
    IWorkGroupRepository workGroupRepository) : IRequestHandler<DeleteWorkGroupCommand>
{
    public async Task Handle(DeleteWorkGroupCommand command, CancellationToken cancellationToken)
    {
        var workGroup = await workGroupRepository.FindAsync(
            command.ProjectId,
            command.WorkGroupId,
            cancellationToken);
        if (workGroup is null)
        {
            throw new EntityException(EntityErrorType.NotFound, WorkGroupMessages.NotFound);
        }

        var hasChildren = await workGroupRepository.HasChildrenAsync(
            workGroup.Id,
            cancellationToken);
        var hasTickets = await workGroupRepository.HasTicketsAsync(
            workGroup.Id,
            cancellationToken);
        if (hasChildren || hasTickets)
        {
            var message = workGroup.ParentWorkGroupId.HasValue ? WorkGroupMessages.MilestoneNotEmpty : WorkGroupMessages.GroupNotEmpty;
            throw new ConflictException(message);
        }

        workGroup.IsDeleted = true;
        workGroup.DeletedAt = DateTime.UtcNow;
        workGroup.DeletedById = currentUser.Id;

        await workGroupRepository.SaveChangesAsync(cancellationToken);
    }
}

using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Interfaces;
using ATMS.Caching.Services.Interfaces;
using ATMS.Project.Contracts.Commands.WorkTickets;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using ATMS.Project.Services.Caching;
using MediatR;

namespace ATMS.Project.Services.Handlers.WorkTickets;

public class DeleteWorkTicketHandler(
    ICurrentUser currentUser,
    IWorkTicketRepository workTicketRepository,
    ICacheService cache) : IRequestHandler<DeleteWorkTicketCommand>
{
    public async Task Handle(DeleteWorkTicketCommand command, CancellationToken cancellationToken)
    {
        var workTicket = await workTicketRepository.FindAsync(command.ProjectId, command.WorkTicketId, cancellationToken);
        if (workTicket is null)
        {
            throw new EntityException(EntityErrorType.NotFound, WorkTicketMessages.NotFound);
        }

        workTicket.IsDeleted = true;
        workTicket.DeletedAt = DateTime.UtcNow;
        workTicket.DeletedById = currentUser.Id;

        await workTicketRepository.SaveChangesAsync(cancellationToken);
        await cache.RemoveWorkTicketAsync(workTicket.Id, cancellationToken);
    }
}

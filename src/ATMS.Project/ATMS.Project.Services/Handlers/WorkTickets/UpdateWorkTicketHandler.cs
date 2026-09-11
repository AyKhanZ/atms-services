using ATMS.Application.Exceptions.Entity;
using ATMS.Caching.Services.Interfaces;
using ATMS.Project.Contracts.Commands.WorkTickets;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using ATMS.Project.Services.Caching;
using AutoMapper;
using MediatR;

namespace ATMS.Project.Services.Handlers.WorkTickets;

public class UpdateWorkTicketHandler(
    IMapper mapper,
    IWorkTicketRepository workTicketRepository,
    IWorkTaskRepository workTaskRepository,
    ICacheService cache) : IRequestHandler<UpdateWorkTicketCommand>
{
    public async Task Handle(UpdateWorkTicketCommand command, CancellationToken cancellationToken)
    {
        var workTicket = await workTicketRepository.FindAsync(command.ProjectId, command.WorkTicketId, cancellationToken);
        if (workTicket is null)
        {
            throw new EntityException(EntityErrorType.NotFound, WorkTicketMessages.NotFound);
        }

        mapper.Map(command, workTicket);

        await workTicketRepository.SaveChangesAsync(cancellationToken);

        var workTaskIds = await workTaskRepository.GetIdsByTicketsAsync([workTicket.Id], cancellationToken);
        await cache.RemoveWorkTicketAsync(workTicket.Id, cancellationToken);
        await cache.RemoveWorkTasksAsync(workTaskIds, cancellationToken);
    }
}

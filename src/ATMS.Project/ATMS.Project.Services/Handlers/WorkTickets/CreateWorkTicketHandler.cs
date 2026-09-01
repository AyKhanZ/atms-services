using ATMS.Data.Enums;
using ATMS.Project.Contracts.Commands.WorkTickets;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Data.Services.Interfaces;
using AutoMapper;
using MediatR;

namespace ATMS.Project.Services.Handlers.WorkTickets;

public class CreateWorkTicketHandler(
    IMapper mapper,
    IWorkTicketRepository workTicketRepository,
    IEntityCodeGenerator codeGenerator) : IRequestHandler<CreateWorkTicketCommand, Guid>
{
    public async Task<Guid> Handle(CreateWorkTicketCommand command, CancellationToken cancellationToken)
    {
        var workTicket = mapper.Map<WorkTicket>(command);
        workTicket.Id = Guid.NewGuid();
        workTicket.Code = await codeGenerator.GetNextAsync(cancellationToken);
        workTicket.WorkTicketStatusId = (int)WorkTicketStatusEnum.New;
        workTicket.StatusId = (int)WorkTaskStatusEnum.New;

        await workTicketRepository.CreateAsync(workTicket, cancellationToken);

        return workTicket.Id;
    }
}

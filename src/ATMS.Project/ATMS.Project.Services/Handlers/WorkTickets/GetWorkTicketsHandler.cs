using ATMS.Application.Exceptions.Entity;
using ATMS.Data.Criteria;
using ATMS.Project.Contracts.Models.WorkTickets;
using ATMS.Project.Contracts.Requests.WorkTickets;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Data.Criteria.WorkTickets;
using ATMS.Project.Data.Entities;
using ATMS.Project.Services.Resources;
using AutoMapper;
using MediatR;

namespace ATMS.Project.Services.Handlers.WorkTickets;

public class GetWorkTicketsHandler(
    IWorkProjectRepository workProjectRepository,
    IWorkTicketRepository workTicketRepository,
    IMapper mapper) : IRequestHandler<GetWorkTicketsRequest, KeysetPagedResult<WorkTicketModel>>
{
    public async Task<KeysetPagedResult<WorkTicketModel>> Handle(
        GetWorkTicketsRequest request,
        CancellationToken cancellationToken)
    {
        if (!await workProjectRepository.IsExistAsync(project => project.Id == request.ProjectId, cancellationToken))
        {
            throw new EntityException(EntityErrorType.NotFound, WorkProjectMessages.NotFound);
        }

        var criteria = new WorkTicketsByProjectCriteria(request.ProjectId, request.MilestoneId);
        var pagination = new KeysetPaginationCriteria<WorkTicket>(
            request.Cursor,
            request.PageSize,
            request.SortDirection);
        var workTickets = await workTicketRepository.GetManyAsync(criteria, pagination, cancellationToken);

        return workTickets.Map(mapper.Map<WorkTicketModel>);
    }
}

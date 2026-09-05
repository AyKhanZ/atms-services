using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Localization;
using ATMS.Caching.Constants;
using ATMS.Caching.Services.Interfaces;
using ATMS.Project.Contracts.Models.WorkTickets;
using ATMS.Project.Contracts.Requests.WorkTickets;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using AutoMapper;
using MediatR;

namespace ATMS.Project.Services.Handlers.WorkTickets;

public class GetWorkTicketHandler(
    IWorkTicketRepository workTicketRepository,
    IWorkTaskRepository workTaskRepository,
    ICacheService cache,
    IMapper mapper) : IRequestHandler<GetWorkTicketRequest, WorkTicketModel>
{
    public async Task<WorkTicketModel> Handle(GetWorkTicketRequest request, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeys.Project.TicketById(request.WorkTicketId, CultureHelper.CurrentLanguage);

        var workTicket = await cache.GetOrSetAsync(
            cacheKey,
            async () =>
            {
                var workTicket = await workTicketRepository.GetAsync(
                    request.ProjectId,
                    request.WorkTicketId,
                    cancellationToken)
                    ?? throw new EntityException(EntityErrorType.NotFound, WorkTicketMessages.NotFound);

                return mapper.Map<WorkTicketModel>(workTicket);
            },
            CacheTtl.Entity,
            cancellationToken)
            ?? throw new EntityException(EntityErrorType.NotFound, WorkTicketMessages.NotFound);

        if (workTicket.WorkProjectId != request.ProjectId)
        {
            throw new EntityException(EntityErrorType.NotFound, WorkTicketMessages.NotFound);
        }

        var progress = await workTaskRepository.GetProgressByTicketAsync([workTicket.Id], cancellationToken);
        if (progress.TryGetValue(workTicket.Id, out var taskProgress))
        {
            workTicket.TotalTaskCount = taskProgress.Total;
            workTicket.DoneTaskCount = taskProgress.Done;
        }

        return workTicket;
    }
}

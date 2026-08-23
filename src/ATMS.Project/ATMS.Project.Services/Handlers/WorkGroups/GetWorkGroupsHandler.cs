using ATMS.Application.Exceptions.Entity;
using ATMS.Project.Contracts.Models.WorkGroups;
using ATMS.Project.Contracts.Requests.WorkGroups;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using AutoMapper;
using MediatR;

namespace ATMS.Project.Services.Handlers.WorkGroups;

public class GetWorkGroupsHandler(
    IWorkProjectRepository workProjectRepository,
    IWorkGroupRepository workGroupRepository,
    IMapper mapper) : IRequestHandler<GetWorkGroupsRequest, WorkGroupModel[]>
{
    public async Task<WorkGroupModel[]> Handle(GetWorkGroupsRequest request, CancellationToken cancellationToken)
    {
        if (!await workProjectRepository.IsExistAsync(project => project.Id == request.ProjectId, cancellationToken))
        {
            throw new EntityException(EntityErrorType.NotFound, WorkProjectMessages.NotFound);
        }

        var queryResult = await workGroupRepository.GetGroupsAsync(request.ProjectId, cancellationToken);
        var groups = mapper.Map<WorkGroupModel[]>(queryResult.Groups);
        ApplyTicketCounts(groups, queryResult.TicketCounts);

        return groups;
    }

    private static void ApplyTicketCounts(WorkGroupModel[] groups, IReadOnlyDictionary<Guid, int> ticketCounts)
    {
        foreach (var group in groups)
        {
            group.TicketCount = ticketCounts.GetValueOrDefault(group.Id);

            foreach (var milestone in group.Milestones)
            {
                milestone.TicketCount = ticketCounts.GetValueOrDefault(milestone.Id);
            }
        }
    }
}

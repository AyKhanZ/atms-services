using ATMS.Application.Interfaces;
using ATMS.Data.Criteria;
using ATMS.Project.Contracts.Models.WorkTaskBoard;
using ATMS.Project.Contracts.Requests.WorkTaskBoard;
using ATMS.Project.Data.Criteria.WorkProjectParticipants;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using AutoMapper;
using MediatR;

namespace ATMS.Project.Services.Handlers.WorkTaskBoard;

public class GetWorkTaskBoardAssigneesHandler(
    ICurrentUser currentUser,
    IWorkTaskBoardRepository workTaskBoardRepository,
    IMapper mapper) : IRequestHandler<GetWorkTaskBoardAssigneesRequest, WorkTaskBoardAssigneeModel[]>
{
    public async Task<WorkTaskBoardAssigneeModel[]> Handle(GetWorkTaskBoardAssigneesRequest request, CancellationToken cancellationToken)
    {
        var filter = mapper.Map<WorkTaskBoardAssigneesFilter>(request);
        var criteria = filter.And(new ExceptSuperAdminCriteria<WorkProjectParticipant>(
            currentUser.RoleId,
            new ParticipantsOfMyProjectsCriteria(currentUser.Id)));

        var people = await workTaskBoardRepository.GetAssigneesAsync(criteria, cancellationToken);

        return mapper.Map<WorkTaskBoardAssigneeModel[]>(people);
    }
}

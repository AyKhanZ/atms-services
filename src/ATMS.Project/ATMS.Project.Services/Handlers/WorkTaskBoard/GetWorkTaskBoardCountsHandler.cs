using ATMS.Data.Criteria;
using ATMS.Project.Data.Entities;
using ATMS.Application.Interfaces;
using ATMS.Project.Contracts.Requests.WorkTaskBoard;
using ATMS.Project.Data.Criteria.WorkTasks;
using ATMS.Project.Data.Repositories.Interfaces;
using AutoMapper;
using MediatR;

namespace ATMS.Project.Services.Handlers.WorkTaskBoard;

public class GetWorkTaskBoardCountsHandler(
    ICurrentUser currentUser,
    IWorkTaskBoardRepository workTaskBoardRepository,
    IMapper mapper) : IRequestHandler<GetWorkTaskBoardCountsRequest, Dictionary<int, int>>
{
    public Task<Dictionary<int, int>> Handle(GetWorkTaskBoardCountsRequest request, CancellationToken cancellationToken)
    {
        var filter = mapper.Map<WorkTaskBoardFilter>(request);
        var criteria = filter
            .And(new WorkTasksOfLiveWorkCriteria())
            .And(new ExceptSuperAdminCriteria<WorkTask>(
                currentUser.RoleId,
                new WorkTasksOfMyProjectsCriteria(currentUser.Id)));

        return workTaskBoardRepository.GetCountsByStatusAsync(criteria, cancellationToken);
    }
}

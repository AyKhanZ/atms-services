using ATMS.Application.Interfaces;
using ATMS.Data.Criteria;
using ATMS.Data.Criteria.Interfaces;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Models.WorkTasks;
using ATMS.Project.Contracts.Requests.WorkTaskBoard;
using ATMS.Project.Data.Criteria.WorkTasks;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using AutoMapper;
using MediatR;

namespace ATMS.Project.Services.Handlers.WorkTaskBoard;

public class GetWorkTaskBoardHandler(
    ICurrentUser currentUser,
    IWorkTaskBoardRepository workTaskBoardRepository,
    IMapper mapper) : IRequestHandler<GetWorkTaskBoardRequest, KeysetPagedResult<WorkTaskModel>>
{
    public async Task<KeysetPagedResult<WorkTaskModel>> Handle(GetWorkTaskBoardRequest request, CancellationToken cancellationToken)
    {
        var filter = mapper.Map<WorkTaskBoardFilter>(request);
        var criteria = filter.And(new ExceptSuperAdminCriteria<WorkTask>(
            currentUser.RoleId,
            new WorkTasksOfMyProjectsCriteria(currentUser.Id)));
        var pagination = Paginate(request);

        var result = await workTaskBoardRepository.GetManyAsync(criteria, pagination, cancellationToken);
        var page = result.Page.Map(mapper.Map<WorkTaskModel>);

        foreach (var task in page.Items)
        {
            if (result.SubtaskProgress.TryGetValue(task.Id, out var progress))
            {
                task.SubtaskCount = progress.Total;
                task.DoneSubtaskCount = progress.Done;
            }
        }

        return page;
    }

    /// <summary>
    /// Each order has its own key: the board's rank, the close date, the deadline or the priority.
    /// Tasks without a deadline stay at the end whichever way the list is turned.
    /// </summary>
    private static IKeysetPagination<WorkTask> Paginate(GetWorkTaskBoardRequest request)
    {
        var sort = Enum.IsDefined((WorkTaskBoardSortEnum)request.Sort)
            ? (WorkTaskBoardSortEnum)request.Sort
            : WorkTaskBoardSortEnum.Rank;

        return sort switch
        {
            WorkTaskBoardSortEnum.DoneAt => new KeysetPaginationCriteria<WorkTask, DateTime?>(
                request.Cursor, request.PageSize, request.SortDirection,
                task => task.DoneAt, task => task.Id, emptyKeysLast: true),
            WorkTaskBoardSortEnum.Deadline => new KeysetPaginationCriteria<WorkTask, DateTime?>(
                request.Cursor, request.PageSize, request.SortDirection,
                task => task.Deadline, task => task.Id, emptyKeysLast: true),
            WorkTaskBoardSortEnum.Priority => new KeysetPaginationCriteria<WorkTask, int>(
                request.Cursor, request.PageSize, request.SortDirection,
                task => task.PriorityId, task => task.Id),
            _ => new KeysetPaginationCriteria<WorkTask, string>(
                request.Cursor, request.PageSize, request.SortDirection,
                task => task.Rank, task => task.Id)
        };
    }
}

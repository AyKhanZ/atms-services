using ATMS.Application.Exceptions.Entity;
using ATMS.Data.Criteria;
using ATMS.Project.Contracts.Models.WorkTasks;
using ATMS.Project.Contracts.Requests.WorkTasks;
using ATMS.Project.Data.Criteria.WorkTasks;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace ATMS.Project.Services.Handlers.WorkTasks;

public class GetWorkTasksHandler(
    IWorkProjectRepository workProjectRepository,
    IWorkTaskRepository workTaskRepository,
    IMapper mapper) : IRequestHandler<GetWorkTasksRequest, KeysetPagedResult<WorkTaskModel>>
{
    public async Task<KeysetPagedResult<WorkTaskModel>> Handle(GetWorkTasksRequest request, CancellationToken cancellationToken)
    {
        if (request.RootTasksOnly && request.ParentWorkTaskId.HasValue)
        {
            throw new ValidationException(
            [
                new ValidationFailure(nameof(GetWorkTasksRequest.ParentWorkTaskId), WorkTaskMessages.HierarchyFilterConflict)
            ]);
        }

        if (!await workProjectRepository.IsExistAsync(project => project.Id == request.ProjectId, cancellationToken))
        {
            throw new EntityException(EntityErrorType.NotFound, WorkProjectMessages.NotFound);
        }

        var criteria = new WorkTasksByProjectCriteria(request.ProjectId, request.WorkTicketId, request.ParentWorkTaskId, request.RootTasksOnly);
        var pagination = new KeysetPaginationCriteria<WorkTask>(request.Cursor, request.PageSize, request.SortDirection);
        var result = await workTaskRepository.GetManyAsync(criteria, pagination, cancellationToken);
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
}

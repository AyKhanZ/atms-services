using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Exceptions.Enums;
using ATMS.Application.Localization;
using ATMS.Caching.Constants;
using ATMS.Caching.Services.Interfaces;
using ATMS.Project.Contracts.Models.WorkTasks;
using ATMS.Project.Contracts.Requests.WorkTasks;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using AutoMapper;
using MediatR;

namespace ATMS.Project.Services.Handlers.WorkTasks;

public sealed class GetWorkTaskHandler(
    IWorkTaskRepository workTaskRepository,
    ICommentRepository commentRepository,
    ICacheService cache,
    IMapper mapper) : IRequestHandler<GetWorkTaskRequest, WorkTaskModel>
{
    public async Task<WorkTaskModel> Handle(GetWorkTaskRequest request, CancellationToken cancellationToken)
    {
        var workTask = await cache.GetOrSetAsync(
            CacheKeys.Project.TaskById(request.WorkTaskId, CultureHelper.CurrentLanguage),
            async () =>
            {
                var entity = await workTaskRepository.GetAsync(
                    request.ProjectId,
                    request.WorkTaskId,
                    cancellationToken)
                    ?? throw new EntityException(EntityErrorTypeEnum.NotFound, WorkTaskMessages.NotFound);

                return mapper.Map<WorkTaskModel>(entity);
            },
            CacheTtl.ActiveItem,
            cancellationToken)
            ?? throw new EntityException(EntityErrorTypeEnum.NotFound, WorkTaskMessages.NotFound);

        if (workTask.WorkProjectId != request.ProjectId)
        {
            throw new EntityException(EntityErrorTypeEnum.NotFound, WorkTaskMessages.NotFound);
        }

        var progress = await workTaskRepository.GetProgressAsync(workTask.Id, cancellationToken);
        workTask.SubtaskCount = progress.Total;
        workTask.DoneSubtaskCount = progress.Done;
        workTask.CommentsCount = await commentRepository.CountAsync(workTask.Id, cancellationToken);

        return workTask;
    }
}

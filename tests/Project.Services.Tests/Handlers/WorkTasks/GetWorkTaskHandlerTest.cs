using ATMS.Caching.Constants;
using ATMS.Application.Localization;
using ATMS.Project.Contracts.Models.WorkTasks;
using ATMS.Project.Contracts.Requests.WorkTasks;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Models.WorkTasks;
using ATMS.Project.Services.Handlers.WorkTasks;
using Moq;

namespace Project.Services.Tests.Handlers.WorkTasks;

public class GetWorkTaskHandlerTest : BaseHandlerTest
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Handle_UsesActiveItemCacheAndAlwaysRefreshesProgress(bool cacheHit)
    {
        var request = new GetWorkTaskRequest { ProjectId = Guid.NewGuid(), WorkTaskId = Guid.NewGuid() };
        var model = new WorkTaskModel
        {
            Id = request.WorkTaskId,
            WorkProjectId = request.ProjectId,
            Code = "42",
            Title = "Task",
            WorkTicketCode = "10",
            WorkTicketTitle = "Ticket",
            MilestoneTitle = "Milestone",
            GroupTitle = "Group",
            Status = new(),
            Priority = new()
        };
        if (cacheHit)
        {
            SetupCacheHit(model);
        }
        else
        {
            SetupCacheMiss<WorkTaskModel>();
            var entity = new WorkTask { Id = request.WorkTaskId };
            WorkTaskRepositoryMock.Setup(repository => repository.GetAsync(request.ProjectId, request.WorkTaskId, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
            MapperMock.Setup(mapper => mapper.Map<WorkTaskModel>(entity)).Returns(model);
        }
        WorkTaskRepositoryMock.Setup(repository => repository.GetProgressAsync(request.WorkTaskId, It.IsAny<CancellationToken>())).ReturnsAsync(new WorkTaskProgress(3, 2));
        var handler = new GetWorkTaskHandler(WorkTaskRepositoryMock.Object, CacheServiceMock.Object, MapperMock.Object);

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(3, result.SubtaskCount);
        Assert.Equal(2, result.DoneSubtaskCount);
        CacheServiceMock.Verify(cache => cache.GetOrSetAsync(
            CacheKeys.Project.TaskById(request.WorkTaskId, CultureHelper.CurrentLanguage),
            It.IsAny<Func<Task<WorkTaskModel>>>(),
            CacheTtl.ActiveItem,
            It.IsAny<CancellationToken>()), Times.Once);
        WorkTaskRepositoryMock.Verify(repository => repository.GetAsync(request.ProjectId, request.WorkTaskId, It.IsAny<CancellationToken>()), cacheHit ? Times.Never : Times.Once);
    }
}

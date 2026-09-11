using System.Linq.Expressions;
using ATMS.Data.Criteria;
using ATMS.Project.Contracts.Models.WorkTasks;
using ATMS.Project.Contracts.Requests.WorkTasks;
using ATMS.Project.Data.Criteria.WorkTasks;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Models.WorkTasks;
using ATMS.Project.Services.Handlers.WorkTasks;
using FluentValidation;
using Moq;

namespace Project.Services.Tests.Handlers.WorkTasks;

public class GetWorkTasksHandlerTest : BaseHandlerTest
{
    [Fact]
    public async Task Handle_WhenHierarchyFiltersConflict_ReturnsFieldValidationFailure()
    {
        var request = new GetWorkTasksRequest
        {
            ProjectId = Guid.NewGuid(),
            ParentWorkTaskId = Guid.NewGuid(),
            RootTasksOnly = true
        };
        var handler = new GetWorkTasksHandler(
            WorkProjectRepositoryMock.Object,
            WorkTaskRepositoryMock.Object,
            MapperMock.Object);

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => handler.Handle(request, CancellationToken.None));

        var error = Assert.Single(exception.Errors);
        Assert.Equal(nameof(GetWorkTasksRequest.ParentWorkTaskId), error.PropertyName);
        WorkProjectRepositoryMock.Verify(
            repository => repository.IsExistAsync(
                It.IsAny<Expression<Func<WorkProject, bool>>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_MapsPageAndAppliesBatchedSubtaskProgress()
    {
        var request = new GetWorkTasksRequest { ProjectId = Guid.NewGuid(), WorkTicketId = Guid.NewGuid(), RootTasksOnly = true };
        WorkProjectRepositoryMock.Setup(repository => repository.IsExistAsync(It.IsAny<Expression<Func<WorkProject, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var entity = new WorkTask { Id = Guid.NewGuid(), CreatedAt = DateTime.UtcNow };
        WorkTaskRepositoryMock.Setup(repository => repository.GetManyAsync(
                It.IsAny<WorkTasksByProjectCriteria>(),
                It.IsAny<KeysetPaginationCriteria<WorkTask>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WorkTasksQueryResult(
                new KeysetPagedResult<WorkTask> { Items = [entity], PageSize = 10 },
                new Dictionary<Guid, WorkTaskProgress> { [entity.Id] = new(4, 3) }));
        var model = new WorkTaskModel
        {
            Id = entity.Id,
            Code = "42",
            Title = "Task",
            WorkTicketCode = "10",
            WorkTicketTitle = "Ticket",
            MilestoneTitle = "Milestone",
            GroupTitle = "Group",
            Status = new(),
            Priority = new()
        };
        MapperMock.Setup(mapper => mapper.Map<WorkTaskModel>(entity)).Returns(model);
        var handler = new GetWorkTasksHandler(WorkProjectRepositoryMock.Object, WorkTaskRepositoryMock.Object, MapperMock.Object);

        var result = await handler.Handle(request, CancellationToken.None);

        var task = Assert.Single(result.Items);
        Assert.Equal(4, task.SubtaskCount);
        Assert.Equal(3, task.DoneSubtaskCount);
    }
}

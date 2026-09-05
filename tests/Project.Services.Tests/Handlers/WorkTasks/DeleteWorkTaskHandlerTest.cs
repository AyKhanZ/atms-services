using ATMS.Caching.Constants;
using ATMS.Project.Contracts.Commands.WorkTasks;
using ATMS.Project.Data.Entities;
using ATMS.Project.Services.Handlers.WorkTasks;
using Moq;

namespace Project.Services.Tests.Handlers.WorkTasks;

public class DeleteWorkTaskHandlerTest : BaseHandlerTest
{
    [Fact]
    public async Task Handle_WhenTaskExists_SoftDeletesOnlyTaskAndInvalidatesItsCache()
    {
        var projectId = Guid.NewGuid();
        var task = new WorkTask { Id = Guid.NewGuid(), WorkProjectId = projectId };
        var userId = Guid.NewGuid();
        CurrentUserMock.SetupGet(user => user.Id).Returns(userId);
        WorkTaskRepositoryMock.Setup(repository => repository.FindAsync(projectId, task.Id, It.IsAny<CancellationToken>())).ReturnsAsync(task);
        var handler = new DeleteWorkTaskHandler(CurrentUserMock.Object, WorkTaskRepositoryMock.Object, CacheServiceMock.Object);

        await handler.Handle(new DeleteWorkTaskCommand { ProjectId = projectId, WorkTaskId = task.Id }, CancellationToken.None);

        Assert.True(task.IsDeleted);
        Assert.Equal(userId, task.DeletedById);
        WorkTaskRepositoryMock.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        VerifyAllLocalizedCacheEntriesRemoved(language => CacheKeys.Project.TaskById(task.Id, language));
    }
}

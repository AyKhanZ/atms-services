using ATMS.Caching.Constants;
using ATMS.Project.Contracts.Commands.WorkTasks;
using ATMS.Project.Data.Entities;
using ATMS.Project.Services.Handlers.WorkTasks;
using Moq;

namespace Project.Services.Tests.Handlers.WorkTasks;

public class DeleteWorkTaskHandlerTest : BaseHandlerTest
{
    private readonly Guid _projectId = Guid.NewGuid();

    private DeleteWorkTaskHandler Handler() =>
        new(CurrentUserMock.Object, WorkTaskRepositoryMock.Object, CacheServiceMock.Object);

    private void Found(WorkTask task, params WorkTask[] subtasks)
    {
        WorkTaskRepositoryMock
            .Setup(repository => repository.FindAsync(_projectId, task.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);
        WorkTaskRepositoryMock
            .Setup(repository => repository.FindChildrenAsync(_projectId, task.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subtasks);
    }

    [Fact]
    public async Task Handle_WhenTaskExists_SoftDeletesItAndInvalidatesItsCache()
    {
        var task = new WorkTask { Id = Guid.NewGuid(), WorkProjectId = _projectId };
        var userId = Guid.NewGuid();
        CurrentUserMock.SetupGet(user => user.Id).Returns(userId);
        Found(task);

        await Handler().Handle(new DeleteWorkTaskCommand { ProjectId = _projectId, WorkTaskId = task.Id }, CancellationToken.None);

        Assert.True(task.IsDeleted);
        Assert.Equal(userId, task.DeletedById);
        WorkTaskRepositoryMock.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        VerifyAllLocalizedCacheEntriesRemoved(language => CacheKeys.Project.TaskById(task.Id, language));
    }

    // A subtask cannot outlive its parent.
    [Fact]
    public async Task Handle_WhenTaskHasSubtasks_DeletesThemUnderTheSameMark()
    {
        var task = new WorkTask { Id = Guid.NewGuid(), WorkProjectId = _projectId };
        var subtasks = new[]
        {
            new WorkTask { Id = Guid.NewGuid(), WorkProjectId = _projectId, ParentWorkTaskId = task.Id },
            new WorkTask { Id = Guid.NewGuid(), WorkProjectId = _projectId, ParentWorkTaskId = task.Id }
        };
        CurrentUserMock.SetupGet(user => user.Id).Returns(Guid.NewGuid());
        Found(task, subtasks);

        await Handler().Handle(new DeleteWorkTaskCommand { ProjectId = _projectId, WorkTaskId = task.Id }, CancellationToken.None);

        Assert.All(subtasks, subtask => Assert.True(subtask.IsDeleted));
    }
}

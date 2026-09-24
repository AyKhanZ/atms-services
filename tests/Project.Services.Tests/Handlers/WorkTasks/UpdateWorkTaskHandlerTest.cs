using ATMS.Caching.Constants;
using ATMS.Project.Services.Board;
using ATMS.Project.Contracts.Commands.WorkTasks;
using ATMS.Project.Data.Entities;
using ATMS.Project.Services.Handlers.WorkTasks;
using Moq;

namespace Project.Services.Tests.Handlers.WorkTasks;

public class UpdateWorkTaskHandlerTest : BaseHandlerTest
{
    private readonly Guid _projectId = Guid.NewGuid();
    private readonly Guid _ticketId = Guid.NewGuid();

    public UpdateWorkTaskHandlerTest()
    {
        WorkTaskRepositoryMock
            .Setup(repository => repository.TrySaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    private UpdateWorkTaskHandler Handler() =>
        new(
            MapperMock.Object,
            WorkTaskRepositoryMock.Object,
            CacheServiceMock.Object,
            new WorkTaskBoardPlacementService(WorkTaskRepositoryMock.Object, new WorkTaskBoardPositionService()));

    private WorkTask Existing(Guid? parentId = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            WorkProjectId = _projectId,
            WorkTicketId = _ticketId,
            ParentWorkTaskId = parentId,
        };

    private void SetupTask(WorkTask task, params WorkTask[] children)
    {
        WorkTaskRepositoryMock
            .Setup(repository => repository.FindAsync(_projectId, task.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);
        WorkTaskRepositoryMock
            .Setup(repository => repository.FindChildrenAsync(_projectId, task.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(children);
    }

    [Fact]
    public async Task Handle_WhenTaskExists_SavesAndInvalidatesDetailsCache()
    {
        var task = Existing();
        var children = new[] { Existing(task.Id), Existing(task.Id) };
        SetupTask(task, children);
        var command = new UpdateWorkTaskCommand
        {
            ProjectId = _projectId,
            WorkTaskId = task.Id,
            WorkTicketId = _ticketId,
            Title = "Updated",
            PriorityId = 1,
            StatusId = 2,
        };

        await Handler().Handle(command, CancellationToken.None);

        MapperMock.Verify(mapper => mapper.Map(command, task), Times.Once);
        WorkTaskRepositoryMock.Verify(
            repository => repository.TrySaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
        VerifyAllLocalizedCacheEntriesRemoved(language => CacheKeys.Project.TaskById(task.Id, language));
        foreach (var child in children)
        {
            VerifyAllLocalizedCacheEntriesRemoved(language => CacheKeys.Project.TaskById(child.Id, language));
        }
    }

    [Fact]
    public async Task Handle_MovesTheTaskAndItsSubtasksToTheChosenTicket()
    {
        var task = Existing();
        var children = new[] { Existing(task.Id), Existing(task.Id) };
        SetupTask(task, children);
        var targetTicketId = Guid.NewGuid();

        await Handler().Handle(
            new UpdateWorkTaskCommand
            {
                ProjectId = _projectId,
                WorkTaskId = task.Id,
                WorkTicketId = targetTicketId,
                Title = "Updated",
                PriorityId = 1,
                StatusId = 1,
            },
            CancellationToken.None);

        Assert.Equal(targetTicketId, task.WorkTicketId);
        Assert.Null(task.ParentWorkTaskId);
        // Subtasks live in their parent's ticket, so they have to travel with it.
        Assert.All(children, child => Assert.Equal(targetTicketId, child.WorkTicketId));
    }

    [Fact]
    public async Task Handle_TakesTheTicketFromTheParentAndIgnoresTheRequestedOne()
    {
        var task = Existing();
        SetupTask(task);
        var parent = Existing();
        WorkTaskRepositoryMock
            .Setup(repository => repository.FindParentAsync(_projectId, parent.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(parent);

        await Handler().Handle(
            new UpdateWorkTaskCommand
            {
                ProjectId = _projectId,
                WorkTaskId = task.Id,
                WorkTicketId = Guid.NewGuid(),
                ParentWorkTaskId = parent.Id,
                Title = "Updated",
                PriorityId = 1,
                StatusId = 1,
            },
            CancellationToken.None);

        Assert.Equal(parent.Id, task.ParentWorkTaskId);
        Assert.Equal(parent.WorkTicketId, task.WorkTicketId);
    }

    [Fact]
    public async Task Handle_InvalidatesBothTheOldAndTheNewParent()
    {
        var previousParentId = Guid.NewGuid();
        var task = Existing(previousParentId);
        SetupTask(task);
        var parent = Existing();
        WorkTaskRepositoryMock
            .Setup(repository => repository.FindParentAsync(_projectId, parent.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(parent);

        await Handler().Handle(
            new UpdateWorkTaskCommand
            {
                ProjectId = _projectId,
                WorkTaskId = task.Id,
                ParentWorkTaskId = parent.Id,
                Title = "Updated",
                PriorityId = 1,
                StatusId = 1,
            },
            CancellationToken.None);

        VerifyAllLocalizedCacheEntriesRemoved(language => CacheKeys.Project.TaskById(previousParentId, language));
        VerifyAllLocalizedCacheEntriesRemoved(language => CacheKeys.Project.TaskById(parent.Id, language));
    }
}

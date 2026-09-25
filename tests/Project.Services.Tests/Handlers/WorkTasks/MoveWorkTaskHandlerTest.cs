using ATMS.Project.Services.Board;
using ATMS.Data.Criteria.Interfaces;
using ATMS.Application.Exceptions.Conflict;
using ATMS.Data.Enums;
using System.Resources;
using ATMS.Project.Contracts.Commands.WorkTasks;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Models.WorkTasks;
using ATMS.Project.Services.Handlers.WorkTasks;
using Moq;

namespace Project.Services.Tests.Handlers.WorkTasks;

public class MoveWorkTaskHandlerTest : BaseHandlerTest
{
    private readonly Guid _projectId = Guid.NewGuid();

    private WorkTask NewTask(int status = (int)WorkTaskStatusEnum.New, string rank = "m5") =>
        new() { Id = Guid.NewGuid(), WorkProjectId = _projectId, StatusId = status, Rank = rank };

    public MoveWorkTaskHandlerTest()
    {
        WorkTaskRepositoryMock
            .Setup(repository => repository.TrySaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    private MoveWorkTaskHandler Handler() => new(
        WorkTaskRepositoryMock.Object,
        CacheServiceMock.Object,
        CurrentUserMock.Object,
        new WorkTaskBoardPlacementService(WorkTaskRepositoryMock.Object, new WorkTaskBoardPositionService()));

    private void Found(WorkTask task) => WorkTaskRepositoryMock
        .Setup(repository => repository.FindAsync(_projectId, task.Id, It.IsAny<CancellationToken>()))
        .ReturnsAsync(task);

    [Fact]
    public async Task Handle_WhenDroppedBetweenTwoCards_TakesStatusAndAKeyBetweenThem()
    {
        var task = NewTask();
        var above = Guid.NewGuid();
        var below = Guid.NewGuid();
        Found(task);
        WorkTaskRepositoryMock
            .Setup(repository => repository.GetRanksAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<ICriteria<WorkTask>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, string> { [above] = "m1", [below] = "m3" });

        await Handler().Handle(new MoveWorkTaskCommand
        {
            ProjectId = _projectId,
            WorkTaskId = task.Id,
            StatusId = (int)WorkTaskStatusEnum.InProgress,
            PreviousWorkTaskId = above,
            NextWorkTaskId = below
        }, CancellationToken.None);

        Assert.Equal((int)WorkTaskStatusEnum.InProgress, task.StatusId);
        Assert.True(string.CompareOrdinal("m1", task.Rank) < 0 && string.CompareOrdinal(task.Rank, "m3") < 0);
        WorkTaskRepositoryMock.Verify(repository => repository.TrySaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenDroppedIntoAnEmptyColumn_TakesItsTop()
    {
        // A key from the old column could already be held in the new one: the card gets a fresh place.
        var task = NewTask();
        Found(task);
        WorkTaskRepositoryMock
            .Setup(repository => repository.GetTopPlaceAsync((int)WorkTaskStatusEnum.InProgress, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkTaskBoardPlace?)null);

        await Handler().Handle(new MoveWorkTaskCommand
        {
            ProjectId = _projectId,
            WorkTaskId = task.Id,
            StatusId = (int)WorkTaskStatusEnum.InProgress
        }, CancellationToken.None);

        Assert.NotEqual("m5", task.Rank);
        Assert.False(string.IsNullOrEmpty(task.Rank));
    }

    [Fact]
    public async Task Handle_WhenMovedToDone_StampsTheCloseDateAndClearsItWhenReopened()
    {
        var task = NewTask();
        Found(task);

        await Handler().Handle(new MoveWorkTaskCommand
        {
            ProjectId = _projectId, WorkTaskId = task.Id, StatusId = (int)WorkTaskStatusEnum.Done
        }, CancellationToken.None);
        Assert.NotNull(task.DoneAt);

        await Handler().Handle(new MoveWorkTaskCommand
        {
            ProjectId = _projectId, WorkTaskId = task.Id, StatusId = (int)WorkTaskStatusEnum.New
        }, CancellationToken.None);
        Assert.Null(task.DoneAt);
    }

    [Fact]
    public async Task Handle_WhenAskedToCompleteSubtasks_ClosesOnlyTheOpenOnes()
    {
        var task = NewTask();
        var open = NewTask(rank: "m6");
        var done = NewTask((int)WorkTaskStatusEnum.Done, "m7");
        var closedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        done.DoneAt = closedAt;
        Found(task);
        WorkTaskRepositoryMock
            .Setup(repository => repository.FindChildrenAsync(_projectId, task.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([open, done]);

        await Handler().Handle(new MoveWorkTaskCommand
        {
            ProjectId = _projectId,
            WorkTaskId = task.Id,
            StatusId = (int)WorkTaskStatusEnum.Done,
            CompleteSubtasks = true
        }, CancellationToken.None);

        Assert.Equal((int)WorkTaskStatusEnum.Done, open.StatusId);
        Assert.NotNull(open.DoneAt);
        Assert.Equal(closedAt, done.DoneAt);
    }

    [Fact]
    public async Task Handle_WhenNeighbourIsInaccessible_SavesNothing()
    {
        var task = NewTask();
        Found(task);
        var userId = Guid.NewGuid();
        CurrentUserMock.SetupGet(user => user.Id).Returns(userId);
        WorkTaskRepositoryMock.Setup(repository => repository.GetRanksAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<ICriteria<WorkTask>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, string>());

        await Assert.ThrowsAsync<ConflictException>(() => Handler().Handle(
            new MoveWorkTaskCommand { ProjectId = _projectId, WorkTaskId = task.Id, StatusId = 2, NextWorkTaskId = Guid.NewGuid() },
            CancellationToken.None));

        WorkTaskRepositoryMock.Verify(repository => repository.TrySaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        CacheServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenMovedToDone_IgnoresManualRankNeighbours()
    {
        var task = NewTask();
        Found(task);
        await Handler().Handle(new MoveWorkTaskCommand
        {
            ProjectId = _projectId, WorkTaskId = task.Id, StatusId = 3, NextWorkTaskId = Guid.NewGuid()
        }, CancellationToken.None);
        Assert.Equal("m5", task.Rank);
        Assert.NotNull(task.DoneAt);
        WorkTaskRepositoryMock.Verify(repository => repository.GetRanksAsync(
            It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<ICriteria<WorkTask>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRankIntervalStaysFullAfterRenumbering_AsksForAnotherPositionWithoutSaving()
    {
        var task = NewTask();
        var previous = Guid.NewGuid();
        var next = Guid.NewGuid();
        Found(task);
        WorkTaskRepositoryMock
            .Setup(repository => repository.GetRanksAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<ICriteria<WorkTask>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, string>
            {
                [previous] = "a",
                [next] = "a" + new string('0', 62) + "1"
            });

        var exception = await Assert.ThrowsAsync<ConflictException>(() => Handler().Handle(
            new MoveWorkTaskCommand
            {
                ProjectId = _projectId,
                WorkTaskId = task.Id,
                StatusId = (int)WorkTaskStatusEnum.InProgress,
                PreviousWorkTaskId = previous,
                NextWorkTaskId = next
            }, CancellationToken.None));

        var resources = new ResourceManager("ATMS.Project.Services.Resources.WorkTaskMessages", typeof(MoveWorkTaskHandler).Assembly);
        Assert.Equal(resources.GetString("BoardPositionUnavailable"), exception.Message);
        Assert.Equal("m5", task.Rank);
        // The target column was spread once before giving up.
        WorkTaskRepositoryMock.Verify(
            repository => repository.RenumberColumnAsync((int)WorkTaskStatusEnum.InProgress, It.IsAny<CancellationToken>()),
            Times.Once);
        WorkTaskRepositoryMock.Verify(repository => repository.TrySaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        CacheServiceMock.VerifyNoOtherCalls();
    }
}

using ATMS.Application.Exceptions.Entity;
using ATMS.Data.Enums;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Domain.Board;
using Moq;

namespace Project.Services.Tests.Domain.Board;

public sealed class WorkTaskColumnServiceTest
{
    private static readonly DateTime Now = new(2026, 10, 18, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Earlier = new(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IBoardColumnRepository> _columns = new();

    [Fact]
    public void Assign_WhenTheCategoryStays_KeepsDoneAt()
    {
        var task = new WorkTask
        {
            StatusId = (int)WorkTaskStatusEnum.Done,
            BoardColumnId = 3,
            DoneAt = Earlier
        };

        Service().Assign(task, Column(8, WorkTaskStatusEnum.Done), Now);

        Assert.Equal(8, task.BoardColumnId);
        Assert.Equal((int)WorkTaskStatusEnum.Done, task.StatusId);
        Assert.Equal(Earlier, task.DoneAt);
    }

    [Fact]
    public void Assign_WhenTheTaskIsClosed_SetsDoneAt()
    {
        var task = new WorkTask
        {
            StatusId = (int)WorkTaskStatusEnum.InProgress,
            BoardColumnId = 2
        };

        Service().Assign(task, Column(3, WorkTaskStatusEnum.Done), Now);

        Assert.Equal(Now, task.DoneAt);
        Assert.Equal((int)WorkTaskStatusEnum.Done, task.StatusId);
    }

    [Fact]
    public void Assign_WhenTheTaskIsReopened_ClearsDoneAt()
    {
        var task = new WorkTask
        {
            StatusId = (int)WorkTaskStatusEnum.Done,
            BoardColumnId = 3,
            DoneAt = Earlier
        };

        Service().Assign(task, Column(2, WorkTaskStatusEnum.InProgress), Now);

        Assert.Null(task.DoneAt);
        Assert.Equal(2, task.BoardColumnId);
    }

    [Fact]
    public async Task CloseOpenAsync_PutsOpenTasksIntoTheDefaultDoneColumn()
    {
        var open = new WorkTask { StatusId = (int)WorkTaskStatusEnum.New, BoardColumnId = 1 };
        var closed = new WorkTask
        {
            StatusId = (int)WorkTaskStatusEnum.Done,
            BoardColumnId = 8,
            DoneAt = Earlier
        };
        _columns
            .Setup(repository => repository.FindAsync((int)WorkTaskStatusEnum.Done, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Column(3, WorkTaskStatusEnum.Done));

        await Service().CloseOpenAsync([open, closed], Now, CancellationToken.None);

        Assert.Equal(3, open.BoardColumnId);
        Assert.Equal(Now, open.DoneAt);
        Assert.Equal(8, closed.BoardColumnId);
        Assert.Equal(Earlier, closed.DoneAt);
    }

    [Fact]
    public async Task GetRequiredAsync_WhenTheColumnIsGone_Throws()
    {
        await Assert.ThrowsAsync<EntityException>(() =>
            Service().GetRequiredAsync(7, CancellationToken.None));
    }

    private WorkTaskColumnService Service() => new(_columns.Object);

    private static BoardColumn Column(int id, WorkTaskStatusEnum category) =>
        new() { Id = id, Category = (int)category };
}

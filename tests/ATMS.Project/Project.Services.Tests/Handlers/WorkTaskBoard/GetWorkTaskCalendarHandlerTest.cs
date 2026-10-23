using ATMS.Data.Constants;
using ATMS.Data.Criteria.Interfaces;
using ATMS.Project.Contracts.Models.WorkTasks;
using ATMS.Project.Contracts.Requests.WorkTaskBoard;
using ATMS.Project.Data.Criteria.WorkTasks;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Models.WorkTasks;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Handlers.WorkTaskBoard;
using FluentValidation;
using Moq;

namespace Project.Services.Tests.Handlers.WorkTaskBoard;

public class GetWorkTaskCalendarHandlerTest : BaseHandlerTest
{
    private static readonly DateTime MonthStart = new(2026, 10, 31, 20, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IWorkTaskBoardRepository> _repository = new();

    public GetWorkTaskCalendarHandlerTest()
    {
        CurrentUserMock.SetupGet(user => user.Id).Returns(Guid.NewGuid());
        CurrentUserMock.SetupGet(user => user.RoleId).Returns(RoleIds.Employee);
        MapperMock.Setup(mapper => mapper.Map<WorkTaskBoardFilter>(It.IsAny<GetWorkTaskCalendarRequest>()))
            .Returns(new WorkTaskBoardFilter());
        MapperMock.Setup(mapper => mapper.Map<WorkTaskModel>(It.IsAny<WorkTask>()))
            .Returns<WorkTask>(task => new WorkTaskModel { Id = task.Id, Code = task.Code, Title = task.Title });
    }

    [Fact]
    public async Task Handle_ReturnsEveryDeadlineWithItsTotalAndFirstCards()
    {
        var busy = MonthStart.AddDays(29);
        var quiet = MonthStart.AddDays(3);
        var parent = new WorkTask { Id = Guid.NewGuid(), Code = "41", Title = "Set up the site office" };
        var other = new WorkTask { Id = Guid.NewGuid(), Code = "42", Title = "Order the scaffolding" };
        _repository.Setup(repo => repo.GetCalendarAsync(
                It.IsAny<ICriteria<WorkTask>>(), 6, It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WorkTaskCalendarResult(
                [new WorkTaskDeadlineGroup(quiet, 1, [other]), new WorkTaskDeadlineGroup(busy, 151, [parent])],
                new Dictionary<Guid, WorkTaskProgress> { [parent.Id] = new(4, 1) }));

        var result = await Handler().Handle(Request(MonthStart, MonthStart.AddDays(30)), CancellationToken.None);

        Assert.Equal([quiet, busy], result.Select(group => group.Deadline));
        Assert.Equal([1, 151], result.Select(group => group.Total));
        var card = Assert.Single(result[1].WorkTasks);
        Assert.Equal(parent.Id, card.Id);
        Assert.Equal(4, card.SubtaskCount);
        Assert.Equal(1, card.DoneSubtaskCount);
        Assert.Equal(0, Assert.Single(result[0].WorkTasks).SubtaskCount);
    }

    [Fact]
    public async Task Handle_ReadsOnlyTheCallersProjects_AndOnlyLiveWork()
    {
        var mine = Guid.NewGuid();
        var me = Guid.NewGuid();
        CurrentUserMock.SetupGet(user => user.Id).Returns(me);
        WorkTask[] tasks =
        [
            Task(mine, me, deleted: false),
            Task(mine, me, deleted: true),
            Task(Guid.NewGuid(), Guid.NewGuid(), deleted: false)
        ];
        ICriteria<WorkTask>? used = null;
        _repository.Setup(repo => repo.GetCalendarAsync(
                It.IsAny<ICriteria<WorkTask>>(), It.IsAny<int>(), It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .Callback<ICriteria<WorkTask>, int, IReadOnlyCollection<string>, CancellationToken>((criteria, _, _, _) => used = criteria)
            .ReturnsAsync(new WorkTaskCalendarResult([], new Dictionary<Guid, WorkTaskProgress>()));

        await Handler().Handle(Request(MonthStart, MonthStart.AddDays(30)), CancellationToken.None);

        Assert.NotNull(used);
        Assert.Equal([tasks[0].Id], used.Apply(tasks.AsQueryable()).Select(task => task.Id));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(43)]
    [InlineData(366)]
    public async Task Handle_RangeEmptyOrLongerThanSixWeeks_IsRefused(int days)
    {
        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            Handler().Handle(Request(MonthStart, MonthStart.AddDays(days)), CancellationToken.None));

        Assert.Equal(nameof(GetWorkTaskCalendarRequest.DeadlineTo), Assert.Single(exception.Errors).PropertyName);
        _repository.Verify(repo => repo.GetCalendarAsync(
            It.IsAny<ICriteria<WorkTask>>(), It.IsAny<int>(), It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(28)]
    [InlineData(42)]
    public async Task Handle_MonthOrSixWeekGrid_IsRead(int days)
    {
        _repository.Setup(repo => repo.GetCalendarAsync(
                It.IsAny<ICriteria<WorkTask>>(), It.IsAny<int>(), It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WorkTaskCalendarResult([], new Dictionary<Guid, WorkTaskProgress>()));

        var result = await Handler().Handle(Request(MonthStart, MonthStart.AddDays(days)), CancellationToken.None);

        Assert.Empty(result);
    }

    private GetWorkTaskCalendarHandler Handler() => new(CurrentUserMock.Object, _repository.Object, MapperMock.Object);

    private static GetWorkTaskCalendarRequest Request(DateTime from, DateTime to) => new()
    {
        DeadlineFrom = from,
        DeadlineTo = to
    };

    private static WorkTask Task(Guid projectId, Guid participantUserId, bool deleted) => new()
    {
        Id = Guid.NewGuid(),
        WorkProjectId = projectId,
        WorkProject = new WorkProject
        {
            Id = projectId,
            IsDeleted = deleted,
            WorkProjectParticipants = [new WorkProjectParticipant { UserId = participantUserId }]
        },
        WorkTicket = new WorkTicket()
    };
}

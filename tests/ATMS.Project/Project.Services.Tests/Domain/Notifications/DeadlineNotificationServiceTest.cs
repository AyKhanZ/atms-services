using ATMS.Data.Enums;
using ATMS.Project.Data.Models.Notifications;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Infrastructure;
using ATMS.Project.Services.Models.Notifications;
using ATMS.Project.Services.Domain.Notifications;
using ATMS.Project.Services.Domain.Notifications.Interfaces;
using Moq;

namespace Project.Services.Tests.Domain.Notifications;

public class DeadlineNotificationServiceTest
{
    // 09:00 on 6 October in Baku.
    private static readonly DateTime Now = new(2026, 10, 6, 5, 0, 0, DateTimeKind.Utc);
    private static readonly BusinessTimeZone Baku = new(TimeZoneInfo.FindSystemTimeZoneById("Asia/Baku"));
    private static readonly DateTime StartOfToday = new(2026, 10, 5, 20, 0, 0, DateTimeKind.Utc);

    private readonly Mock<INotificationRepository> _repository = new();
    private readonly Mock<INotificationService> _notifications = new();
    private readonly List<(NotificationDraft Draft, Guid[] Recipients)> _sent = [];

    public DeadlineNotificationServiceTest()
    {
        _repository
            .Setup(repository => repository.GetOpenTasksDueAsync(
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _notifications
            .Setup(service => service.AddRangeAsync(
                It.IsAny<IReadOnlyCollection<NotificationRecipients>>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyCollection<NotificationRecipients>, CancellationToken>((batch, _) =>
                _sent.AddRange(batch.Select(item => (item.Draft, item.UserIds.ToArray()))))
            .Returns(Task.CompletedTask);
    }

    private DeadlineNotificationService Service() => new(_repository.Object, _notifications.Object, Baku);

    private void Due(DateTime fromUtc, params DeadlineTaskRow[] tasks) =>
        _repository
            .Setup(repository => repository.GetOpenTasksDueAsync(fromUtc, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tasks);

    private static DeadlineTaskRow NewTask(DateTime deadline, Guid? assignee, params Guid[] managers) =>
        new(Guid.NewGuid(), Guid.NewGuid(), "Project Alpha", "41", "Payment form", false, deadline, assignee, managers);

    [Fact]
    public async Task RemindAsync_ReadsTodayAndTheLastSevenDaysInBaku()
    {
        await Service().RemindAsync(Now, CancellationToken.None);

        _repository.Verify(repository => repository.GetOpenTasksDueAsync(
            StartOfToday, StartOfToday.AddDays(1), It.IsAny<CancellationToken>()), Times.Once);
        _repository.Verify(repository => repository.GetOpenTasksDueAsync(
            StartOfToday.AddDays(-7), StartOfToday, It.IsAny<CancellationToken>()), Times.Once);
        _repository.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemindAsync_TellsTheAssigneeTheTaskIsDueToday()
    {
        var assignee = Guid.NewGuid();
        var task = NewTask(StartOfToday, assignee, Guid.NewGuid());
        Due(StartOfToday, task);

        await Service().RemindAsync(Now, CancellationToken.None);

        var (draft, recipients) = Assert.Single(_sent);
        Assert.Equal(NotificationTypeEnum.DueToday, draft.Type);
        Assert.Equal(task.ProjectId, draft.ProjectId);
        Assert.Equal(task.Id, draft.EntityId);
        Assert.Null(draft.ActorId);
        Assert.Equal(new DateOnly(2026, 10, 6), draft.Parameters.Deadline);
        Assert.Equal("41", draft.Parameters.TaskCode);
        Assert.Equal($"due-today:{task.Id}:2026-10-06", draft.DedupKey);
        // Due today is the assignee's business; the managers hear only when it is late.
        Assert.Equal([assignee], recipients);
    }

    [Fact]
    public async Task RemindAsync_WhenAnOverdueTaskHasAnAssignee_TellsThemAndTheProjectManagers()
    {
        var assignee = Guid.NewGuid();
        var managers = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var task = NewTask(StartOfToday.AddDays(-1), assignee, managers);
        Due(StartOfToday.AddDays(-7), task);

        await Service().RemindAsync(Now, CancellationToken.None);

        var (draft, recipients) = Assert.Single(_sent);
        Assert.Equal(NotificationTypeEnum.TaskOverdue, draft.Type);
        Assert.Equal(new DateOnly(2026, 10, 5), draft.Parameters.Deadline);
        Assert.Equal($"overdue:{task.Id}:2026-10-05", draft.DedupKey);
        Assert.Equal([assignee, .. managers], recipients);
    }

    [Fact]
    public async Task RemindAsync_WhenAnOverdueTaskIsUnassigned_TellsOnlyTheManagers()
    {
        var manager = Guid.NewGuid();
        Due(StartOfToday.AddDays(-7), NewTask(StartOfToday.AddDays(-2), null, manager));

        await Service().RemindAsync(Now, CancellationToken.None);

        Assert.Equal([manager], Assert.Single(_sent).Recipients);
    }
}

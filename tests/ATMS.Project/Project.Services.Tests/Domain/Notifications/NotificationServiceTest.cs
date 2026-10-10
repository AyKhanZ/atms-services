using ATMS.Data.Criteria;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ATMS.Infrastructure.Options;
using ATMS.Data.Enums;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Models.Notifications;
using ATMS.Project.Data.Models.WorkProjects;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Infrastructure;
using ATMS.Project.Services.Models.Notifications;
using ATMS.Project.Services.Domain.Notifications;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Project.Services.Tests.Domain.Notifications;

public class NotificationServiceTest
{
    private static readonly Guid ProjectId = Guid.NewGuid();
    private static readonly Guid TaskId = Guid.NewGuid();
    private static readonly Guid ActorId = Guid.NewGuid();

    private readonly Mock<INotificationRepository> _notifications = new();
    private readonly Mock<IEmailDeliveryRepository> _emails = new();
    private readonly Mock<IProjectPermissionRepository> _permissions = new();
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<ILogger<NotificationService>> _logger = new();
    private readonly BusinessTimeZone _time = new(TimeZoneInfo.FindSystemTimeZoneById("Asia/Baku"));
    private readonly List<Notification> _added = [];
    private readonly List<EmailDelivery> _queued = [];

    public NotificationServiceTest()
    {
        _logger.Setup(logger => logger.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _notifications
            .Setup(repository => repository.AddRangeAsync(It.IsAny<IEnumerable<Notification>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<Notification>, CancellationToken>((notifications, _) => _added.AddRange(notifications))
            .Returns(Task.CompletedTask);
        _emails
            .Setup(repository => repository.AddRangeAsync(It.IsAny<IEnumerable<EmailDelivery>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<EmailDelivery>, CancellationToken>((deliveries, _) => _queued.AddRange(deliveries))
            .Returns(Task.CompletedTask);
        _emails
            .Setup(repository => repository.CountSinceAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        _emails
            .Setup(repository => repository.CountByUserSinceAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, int>());
        UnreadSince();
        _users
            .Setup(repository => repository.GetManyAsync(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<ACriteria<User>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<Guid> ids, ACriteria<User> _, CancellationToken _) =>
                ids.Select(id => new User { Id = id, IsActive = true }).ToList());
        _notifications
            .Setup(repository => repository.GetDedupKeysAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
    }

    private NotificationService Service(bool sendEmails = false) =>
        new(
            _notifications.Object,
            _emails.Object,
            _permissions.Object,
            _users.Object,
            Options.Create(Configuration(sendEmails).GetSection(nameof(NotificationsOptions)).Get<NotificationsOptions>()!),
            _time,
            _logger.Object);

    private static IConfiguration Configuration(bool sendEmails) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["NotificationsOptions:SendEmails"] = sendEmails.ToString(),
                ["NotificationsOptions:AppUrl"] = "http://localhost:4200"
            })
            .Build();

    private static NotificationDraft Draft(
        NotificationTypeEnum type = NotificationTypeEnum.TaskStatusChanged,
        string? dedupKey = null,
        Guid? projectId = null) =>
        new(
            type,
            projectId ?? ProjectId,
            NotificationEntityTypeEnum.WorkTask,
            TaskId,
            new NotificationParameters { TaskCode = "41", TaskTitle = "Payment form", ToStatusId = 3 })
        {
            ActorId = ActorId,
            DedupKey = dedupKey
        };

    private void Viewers(params (Guid ProjectId, Guid UserId)[] viewers) =>
        _permissions
            .Setup(repository => repository.GetUsersWithPermissionAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<IReadOnlyCollection<Guid>>(),
                ProjectPermissionEnum.ProjectView,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewers.Select(viewer => new ProjectUserRow(viewer.ProjectId, viewer.UserId)).ToArray());

    private void ProjectViewers(params Guid[] userIds) =>
        Viewers(userIds.Select(userId => (ProjectId, userId)).ToArray());

    private void SentToday(int total, params (Guid UserId, int Count)[] byUser)
    {
        _emails
            .Setup(repository => repository.CountSinceAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(total);
        _emails
            .Setup(repository => repository.CountByUserSinceAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(byUser.ToDictionary(item => item.UserId, item => item.Count));
    }

    private void Logged(LogLevel level, string fragment, Times times) =>
        _logger.Verify(logger => logger.Log(
            level,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((state, _) => state.ToString()!.Contains(fragment)),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), times);

    private void UnreadSince(params Notification[] unread) =>
        _notifications
            .Setup(repository => repository.GetUnreadSinceAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(unread);

    private static Notification Unread(Guid userId, int minutesAgo) =>
        new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = (int)NotificationTypeEnum.TaskStatusChanged,
            EntityId = TaskId,
            ActorId = Guid.NewGuid(),
            Parameters = new NotificationParameters { ToStatusId = 2 },
            CreatedAt = DateTime.UtcNow.AddMinutes(-minutesAgo)
        };

    [Fact]
    public async Task AddAsync_WhenTheAuthorIsAmongRecipients_LeavesThemOut()
    {
        var assignee = Guid.NewGuid();
        ProjectViewers(ActorId, assignee);

        await Service().AddAsync(Draft(), [ActorId, assignee], CancellationToken.None);

        Assert.Equal([assignee], _added.Select(notification => notification.UserId));
        _permissions.Verify(repository => repository.GetUsersWithPermissionAsync(
            It.IsAny<IReadOnlyCollection<Guid>>(),
            It.Is<IReadOnlyCollection<Guid>>(userIds => !userIds.Contains(ActorId)),
            ProjectPermissionEnum.ProjectView,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddAsync_WhenOnlyTheAuthorWouldGetIt_QueriesNothing()
    {
        await Service().AddAsync(Draft(), [ActorId, Guid.Empty], CancellationToken.None);

        Assert.Empty(_added);
        _permissions.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task AddAsync_WhenARecipientIsNoLongerInTheProject_GivesItOnlyToParticipants()
    {
        var participant = Guid.NewGuid();
        ProjectViewers(participant);

        await Service().AddAsync(Draft(), [participant, Guid.NewGuid()], CancellationToken.None);

        Assert.Equal([participant], _added.Select(notification => notification.UserId));
    }

    [Fact]
    public async Task AddAsync_WhenNobodyCanSeeTheProject_AddsNothing()
    {
        ProjectViewers();

        await Service().AddAsync(Draft(), [Guid.NewGuid()], CancellationToken.None);

        _notifications.Verify(repository => repository.AddRangeAsync(
            It.IsAny<IEnumerable<Notification>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddAsync_WhenSomeoneIsAddedToTheProject_DoesNotAskTheDatabaseAboutAccessYet()
    {
        // The participant row is saved in the same SaveChanges, so the database cannot see it yet.
        var newParticipant = Guid.NewGuid();
        var draft = new NotificationDraft(
            NotificationTypeEnum.AddedToProject,
            ProjectId,
            NotificationEntityTypeEnum.Project,
            ProjectId,
            new NotificationParameters { ProjectTitle = "Project Alpha" })
        {
            ActorId = ActorId
        };

        await Service().AddAsync(draft, [newParticipant, ActorId], CancellationToken.None);

        Assert.Equal([newParticipant], _added.Select(notification => notification.UserId));
        _permissions.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task AddAsync_WhenTheSameUserIsListedTwice_CreatesOneNotification()
    {
        var assignee = Guid.NewGuid();
        ProjectViewers(assignee);

        await Service().AddAsync(Draft(), [assignee, assignee], CancellationToken.None);

        Assert.Single(_added);
    }

    [Fact]
    public async Task AddAsync_FillsTheNewNotificationFromTheDraft()
    {
        var assignee = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        ProjectViewers(assignee);
        var draft = Draft() with { CommentId = commentId };
        var before = DateTime.UtcNow;

        await Service().AddAsync(draft, [assignee], CancellationToken.None);

        var notification = Assert.Single(_added);
        Assert.NotEqual(Guid.Empty, notification.Id);
        Assert.Equal((int)NotificationTypeEnum.TaskStatusChanged, notification.Type);
        Assert.Equal(ProjectId, notification.WorkProjectId);
        Assert.Equal((int)NotificationEntityTypeEnum.WorkTask, notification.EntityType);
        Assert.Equal(TaskId, notification.EntityId);
        Assert.Equal(commentId, notification.CommentId);
        Assert.Equal(ActorId, notification.ActorId);
        Assert.Equal(draft.Parameters, notification.Parameters);
        Assert.NotSame(draft.Parameters, notification.Parameters);
        Assert.Null(notification.ReadAt);
        Assert.Null(notification.DedupKey);
        Assert.InRange(notification.CreatedAt, before, DateTime.UtcNow);
    }

    [Fact]
    public async Task AddRangeAsync_ChecksEveryProjectInOneQueryAndMatchesThePersonToTheProject()
    {
        var otherProjectId = Guid.NewGuid();
        var inBoth = Guid.NewGuid();
        var onlyInOther = Guid.NewGuid();
        Viewers((ProjectId, inBoth), (otherProjectId, inBoth), (otherProjectId, onlyInOther));

        await Service().AddRangeAsync(
            [
                new NotificationRecipients(Draft(), [inBoth, onlyInOther]),
                new NotificationRecipients(Draft(projectId: otherProjectId), [inBoth, onlyInOther])
            ],
            CancellationToken.None);

        Assert.Equal(
            new[] { (ProjectId, inBoth), (otherProjectId, inBoth), (otherProjectId, onlyInOther) }.Order(),
            _added.Select(notification => (notification.WorkProjectId, notification.UserId)).Order());
        _permissions.Verify(repository => repository.GetUsersWithPermissionAsync(
            It.Is<IReadOnlyCollection<Guid>>(projectIds => projectIds.Count == 2),
            It.IsAny<IReadOnlyCollection<Guid>>(),
            ProjectPermissionEnum.ProjectView,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddAsync_LooksForUnreadOnesFromTheLastTenMinutes()
    {
        var assignee = Guid.NewGuid();
        ProjectViewers(assignee);
        var before = DateTime.UtcNow;

        await Service().AddAsync(Draft(), [assignee], CancellationToken.None);

        _notifications.Verify(repository => repository.GetUnreadSinceAsync(
            It.Is<IReadOnlyCollection<Guid>>(userIds => userIds.SequenceEqual(new[] { assignee })),
            It.Is<IReadOnlyCollection<Guid>>(entityIds => entityIds.SequenceEqual(new[] { TaskId })),
            It.Is<DateTime>(since => since >= before.AddMinutes(-10) && since <= DateTime.UtcNow.AddMinutes(-10)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddAsync_LocksTheTaskBeforeLookingForAnUnreadOneToMergeInto()
    {
        var assignee = Guid.NewGuid();
        ProjectViewers(assignee);
        var steps = new List<string>();
        _notifications
            .Setup(repository => repository.LockForMergeAsync(
                It.Is<IReadOnlyCollection<Guid>>(entityIds => entityIds.SequenceEqual(new[] { TaskId })),
                It.IsAny<CancellationToken>()))
            .Callback(() => steps.Add("lock"))
            .Returns(Task.CompletedTask);
        _notifications
            .Setup(repository => repository.GetUnreadSinceAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .Callback(() => steps.Add("read"))
            .ReturnsAsync([]);

        await Service().AddAsync(Draft(), [assignee], CancellationToken.None);

        // Read before the lock, two changes of one task at once would both find nothing to merge into.
        Assert.Equal(["lock", "read"], steps);
    }

    [Fact]
    public async Task AddAsync_WhenOnlyDeadlineRemindersAreWritten_LocksNothing()
    {
        // A reminder has its own unique key; nothing is merged, so nothing has to wait.
        var assignee = Guid.NewGuid();
        ProjectViewers(assignee);

        await Service().AddAsync(Draft(NotificationTypeEnum.DueToday, "due-today:key"), [assignee], CancellationToken.None);

        _notifications.Verify(repository => repository.LockForMergeAsync(
            It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddAsync_WhenAnUnreadOneOfTheSameKindIsYoungerThanTenMinutes_MergesIntoIt()
    {
        var assignee = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        ProjectViewers(assignee);
        var existing = Unread(assignee, minutesAgo: 3);
        UnreadSince(existing);
        var before = DateTime.UtcNow;

        await Service(sendEmails: true).AddAsync(Draft() with { CommentId = commentId }, [assignee], CancellationToken.None);

        Assert.Empty(_added);
        Assert.InRange(existing.CreatedAt, before, DateTime.UtcNow);
        Assert.Equal(ActorId, existing.ActorId);
        Assert.Equal(commentId, existing.CommentId);
        Assert.Equal(3, existing.Parameters.ToStatusId);
        // The burst was already told: no second email for it.
        Assert.Empty(_queued);
    }

    [Fact]
    public async Task AddAsync_WhenAnUnreadOneIsOfAnotherKind_DoesNotMerge()
    {
        var assignee = Guid.NewGuid();
        ProjectViewers(assignee);
        var assigned = Unread(assignee, minutesAgo: 1);
        assigned.Type = (int)NotificationTypeEnum.TaskAssigned;
        UnreadSince(assigned);

        await Service().AddAsync(Draft(), [assignee], CancellationToken.None);

        Assert.Single(_added);
        Assert.Equal(2, assigned.Parameters.ToStatusId);
    }

    [Fact]
    public async Task AddAsync_WhenOnlySomeRecipientsHaveAnUnreadOne_MergesForThemAndAddsForTheRest()
    {
        var merged = Guid.NewGuid();
        var fresh = Guid.NewGuid();
        ProjectViewers(merged, fresh);
        var older = Unread(merged, minutesAgo: 8);
        var newer = Unread(merged, minutesAgo: 2);
        var olderCreatedAt = older.CreatedAt;
        UnreadSince(older, newer);

        await Service().AddAsync(Draft(), [merged, fresh], CancellationToken.None);

        Assert.Equal([fresh], _added.Select(notification => notification.UserId));
        // Only the newest unread row takes the news; an older one stays as it was.
        Assert.Equal(olderCreatedAt, older.CreatedAt);
        Assert.Equal(2, older.Parameters.ToStatusId);
        Assert.Equal(3, newer.Parameters.ToStatusId);
    }

    [Fact]
    public async Task AddAsync_WhenADeadlineReminderWasAlreadySent_SkipsThatPerson()
    {
        var reminded = Guid.NewGuid();
        var notReminded = Guid.NewGuid();
        ProjectViewers(reminded, notReminded);
        const string dedupKey = "due-today:task:2026-10-02";
        _notifications
            .Setup(repository => repository.GetDedupKeysAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.Is<IReadOnlyCollection<string>>(keys => keys.Contains(dedupKey)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([new NotificationKeyRow(reminded, dedupKey)]);

        await Service().AddAsync(
            Draft(NotificationTypeEnum.DueToday, dedupKey) with { ActorId = null },
            [reminded, notReminded],
            CancellationToken.None);

        var notification = Assert.Single(_added);
        Assert.Equal(notReminded, notification.UserId);
        Assert.Equal(dedupKey, notification.DedupKey);
        Assert.Null(notification.ActorId);
        _notifications.Verify(repository => repository.GetUnreadSinceAsync(
            It.IsAny<IReadOnlyCollection<Guid>>(),
            It.IsAny<IReadOnlyCollection<Guid>>(),
            It.IsAny<DateTime>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(NotificationTypeEnum.TaskAssigned, true)]
    [InlineData(NotificationTypeEnum.Mentioned, true)]
    [InlineData(NotificationTypeEnum.DueToday, true)]
    [InlineData(NotificationTypeEnum.TaskOverdue, true)]
    [InlineData(NotificationTypeEnum.AddedToProject, true)]
    [InlineData(NotificationTypeEnum.TaskStatusChanged, false)]
    [InlineData(NotificationTypeEnum.CommentAdded, false)]
    public async Task AddAsync_QueuesAnEmailOnlyForWorkToActOn(NotificationTypeEnum type, bool emailed)
    {
        var assignee = Guid.NewGuid();
        ProjectViewers(assignee);

        await Service(sendEmails: true).AddAsync(Draft(type), [assignee], CancellationToken.None);

        var notification = Assert.Single(_added);
        if (emailed)
        {
            var delivery = Assert.Single(_queued);
            Assert.Equal(notification.Id, delivery.NotificationId);
            Assert.Equal((int)DeliveryStatusEnum.Pending, delivery.Status);
            Assert.Equal(notification.CreatedAt, delivery.NextAttemptAt);
        }
        else
        {
            Assert.Empty(_queued);
        }
    }

    [Fact]
    public async Task AddAsync_WhenEmailsAreOff_WritesNoEmailRowsAtAll()
    {
        var assignee = Guid.NewGuid();
        ProjectViewers(assignee);

        await Service(sendEmails: false).AddAsync(Draft(NotificationTypeEnum.TaskAssigned), [assignee], CancellationToken.None);

        Assert.Single(_added);
        _emails.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task AddAsync_WhenTheRecipientIsInactive_KeepsTheNotificationAndSkipsTheEmail()
    {
        var assignee = Guid.NewGuid();
        ProjectViewers(assignee);
        // the active-only query finds nobody
        _users
            .Setup(repository => repository.GetManyAsync(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<ACriteria<User>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        await Service(sendEmails: true).AddAsync(Draft(NotificationTypeEnum.DueToday), [assignee], CancellationToken.None);

        Assert.Equal(assignee, Assert.Single(_added).UserId);
        Assert.Empty(_queued);
    }

    [Fact]
    public async Task AddAsync_WhenTheRecipientAlreadyHasFiftyEmailsToday_KeepsTheNotificationAndSkipsTheEmail()
    {
        var assignee = Guid.NewGuid();
        ProjectViewers(assignee);
        SentToday(50, (assignee, 50));
        var start = _time.StartOfDayUtc(_time.Today(DateTime.UtcNow));

        await Service(sendEmails: true).AddAsync(Draft(NotificationTypeEnum.TaskAssigned), [assignee], CancellationToken.None);

        Assert.Equal(assignee, Assert.Single(_added).UserId);
        Assert.Empty(_queued);
        _emails.Verify(repository => repository.AddRangeAsync(
            It.IsAny<IEnumerable<EmailDelivery>>(), It.IsAny<CancellationToken>()), Times.Never);
        _emails.Verify(repository => repository.CountSinceAsync(
            It.Is<DateTime>(since => since == start || since == _time.StartOfDayUtc(_time.Today(DateTime.UtcNow))),
            It.IsAny<CancellationToken>()), Times.Once);
        _emails.Verify(repository => repository.CountByUserSinceAsync(
            It.Is<IReadOnlyCollection<Guid>>(ids => ids.Count == 1 && ids.Contains(assignee)),
            It.Is<DateTime>(since => since == start || since == _time.StartOfDayUtc(_time.Today(DateTime.UtcNow))),
            It.IsAny<CancellationToken>()), Times.Once);
        Logged(LogLevel.Information, "1 notification emails skipped, daily limit per recipient", Times.Once());
        Logged(LogLevel.Warning, "Daily email limit of the service reached", Times.Never());
    }

    [Fact]
    public async Task AddAsync_WhenTheServiceAlreadySentThreeHundredEmailsToday_QueuesNone()
    {
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        ProjectViewers(first, second);
        SentToday(300);

        await Service(sendEmails: true).AddAsync(
            Draft(NotificationTypeEnum.TaskAssigned),
            [first, second],
            CancellationToken.None);

        Assert.Equal([first, second], _added.Select(notification => notification.UserId));
        Assert.Empty(_queued);
        _emails.Verify(repository => repository.CountSinceAsync(
            It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        _emails.Verify(repository => repository.CountByUserSinceAsync(
            It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        Logged(LogLevel.Warning, "Daily email limit of the service reached", Times.Never());
        Logged(LogLevel.Information, "daily limit per recipient", Times.Never());
    }

    [Fact]
    public async Task AddRangeAsync_WhenTwoEmailsRemainForTheRecipient_QueuesThoseTwoAndSkipsTheThird()
    {
        var userId = Guid.NewGuid();
        ProjectViewers(userId);
        // 48 already today, two of the daily 50 are still free
        SentToday(48, (userId, 48));

        await Service(sendEmails: true).AddRangeAsync(
            [
                new NotificationRecipients(Draft(NotificationTypeEnum.DueToday, "due:1"), [userId]),
                new NotificationRecipients(Draft(NotificationTypeEnum.DueToday, "due:2"), [userId]),
                new NotificationRecipients(Draft(NotificationTypeEnum.DueToday, "due:3"), [userId])
            ],
            CancellationToken.None);

        Assert.Equal(3, _added.Count);
        Assert.Equal(
            _added.Take(2).Select(notification => notification.Id),
            _queued.Select(delivery => delivery.NotificationId));
        _emails.Verify(repository => repository.CountSinceAsync(
            It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        _emails.Verify(repository => repository.CountByUserSinceAsync(
            It.Is<IReadOnlyCollection<Guid>>(ids => ids.Count == 1 && ids.Contains(userId)),
            It.IsAny<DateTime>(),
            It.IsAny<CancellationToken>()), Times.Once);
        Logged(LogLevel.Information, "1 notification emails skipped, daily limit per recipient", Times.Once());
        Logged(LogLevel.Warning, "Daily email limit of the service reached", Times.Never());
    }

    [Fact]
    public async Task AddRangeAsync_WarnsOnlyInTheBatchThatReachesTheServiceLimit()
    {
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        ProjectViewers(first, second);
        _emails
            .SetupSequence(repository => repository.CountSinceAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(299)
            .ReturnsAsync(300);

        await Service(sendEmails: true).AddRangeAsync(
            [
                new NotificationRecipients(Draft(NotificationTypeEnum.TaskAssigned), [first]),
                new NotificationRecipients(Draft(NotificationTypeEnum.Mentioned), [second])
            ],
            CancellationToken.None);

        Assert.Equal(2, _added.Count);
        Assert.Equal(_added[0].Id, Assert.Single(_queued).NotificationId);
        Logged(LogLevel.Warning, "Daily email limit of the service reached", Times.Once());
        Logged(LogLevel.Information, "daily limit per recipient", Times.Never());

        await Service(sendEmails: true).AddAsync(
            Draft(NotificationTypeEnum.DueToday, "due:later"),
            [first],
            CancellationToken.None);

        Assert.Equal(3, _added.Count);
        Assert.Single(_queued);
        _emails.Verify(repository => repository.CountSinceAsync(
            It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        Logged(LogLevel.Warning, "Daily email limit of the service reached", Times.Once());
    }

    [Fact]
    public async Task AddRangeAsync_WhenEmailsAreOff_DoesNotReadTheDailyCounters()
    {
        var assignee = Guid.NewGuid();
        ProjectViewers(assignee);

        await Service(sendEmails: false).AddRangeAsync(
            [
                new NotificationRecipients(Draft(NotificationTypeEnum.DueToday, "due:1"), [assignee]),
                new NotificationRecipients(Draft(NotificationTypeEnum.TaskAssigned), [assignee])
            ],
            CancellationToken.None);

        Assert.Equal(2, _added.Count);
        _emails.Verify(repository => repository.CountSinceAsync(
            It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
        _emails.Verify(repository => repository.CountByUserSinceAsync(
            It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

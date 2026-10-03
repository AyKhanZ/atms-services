using ATMS.Application.Realtime;
using ATMS.Data.Enums;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;

namespace Project.Services.Tests.Realtime;

// The bell of every recipient, pushed after the commit.
public sealed class NotificationRealtimeInterceptorTest
{
    [PostgresFact]
    public async Task NewNotifications_PushTheNewestOneAndTheUnreadCountToEachRecipient()
    {
        await using var connection = await OpenTemporaryNotificationsAsync();
        var publisher = new RecordingPublisher();
        await using var context = CreateContext(connection, publisher);
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        var alreadyUnread = NewNotification(first);
        var alreadyRead = NewNotification(first);
        alreadyRead.ReadAt = DateTime.UtcNow;
        context.Notifications.AddRange(alreadyUnread, alreadyRead);
        await context.SaveChangesAsync();
        publisher.Events.Clear();

        var forFirst = NewNotification(first);
        var forSecond = NewNotification(second);
        context.Notifications.AddRange(forFirst, forSecond);
        await context.SaveChangesAsync();

        Assert.Equal(2, publisher.Events.Count);
        var firstEvent = Assert.Single(publisher.Events, sent => sent.UserId == first);
        Assert.Equal(RealtimeEventNames.NotificationCreated, firstEvent.EventName);
        Assert.Equal(new NotificationCreatedEvent(forFirst.Id, 2), firstEvent.Payload);
        var secondEvent = Assert.Single(publisher.Events, sent => sent.UserId == second);
        Assert.Equal(new NotificationCreatedEvent(forSecond.Id, 1), secondEvent.Payload);
    }

    [PostgresFact]
    public async Task MergedNotification_IsPushedAsNewAgain()
    {
        await using var connection = await OpenTemporaryNotificationsAsync();
        var publisher = new RecordingPublisher();
        await using var context = CreateContext(connection, publisher);
        var notification = NewNotification(Guid.NewGuid());
        context.Notifications.Add(notification);
        await context.SaveChangesAsync();
        publisher.Events.Clear();

        notification.CreatedAt = DateTime.UtcNow;
        notification.Parameters = notification.Parameters with { ToStatusId = 3 };
        await context.SaveChangesAsync();

        var sent = Assert.Single(publisher.Events);
        Assert.Equal(RealtimeEventNames.NotificationCreated, sent.EventName);
        Assert.Equal(new NotificationCreatedEvent(notification.Id, 1), sent.Payload);
    }

    [PostgresFact]
    public async Task ReadAndUnread_PushTheNewCountOnly()
    {
        await using var connection = await OpenTemporaryNotificationsAsync();
        var publisher = new RecordingPublisher();
        await using var context = CreateContext(connection, publisher);
        var userId = Guid.NewGuid();
        var first = NewNotification(userId);
        var second = NewNotification(userId);
        context.Notifications.AddRange(first, second);
        await context.SaveChangesAsync();
        publisher.Events.Clear();

        first.ReadAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        first.ReadAt = null;
        await context.SaveChangesAsync();

        Assert.Equal(
            [
                (userId, RealtimeEventNames.NotificationRead, (object)new NotificationReadEvent(1)),
                (userId, RealtimeEventNames.NotificationRead, new NotificationReadEvent(2))
            ],
            publisher.Events);
    }

    [PostgresFact]
    public async Task Transaction_PushesOncePerPersonAfterTheCommit()
    {
        await using var connection = await OpenTemporaryNotificationsAsync();
        var publisher = new RecordingPublisher();
        await using var context = CreateContext(connection, publisher);
        var userId = Guid.NewGuid();
        await using var transaction = await context.Database.BeginTransactionAsync();

        var first = NewNotification(userId);
        context.Notifications.Add(first);
        await context.SaveChangesAsync();
        var second = NewNotification(userId);
        context.Notifications.Add(second);
        await context.SaveChangesAsync();
        Assert.Empty(publisher.Events);
        await transaction.CommitAsync();

        var sent = Assert.Single(publisher.Events);
        Assert.Equal(new NotificationCreatedEvent(second.Id, 2), sent.Payload);
    }

    [PostgresFact]
    public async Task Rollback_PushesNothing()
    {
        await using var connection = await OpenTemporaryNotificationsAsync();
        var publisher = new RecordingPublisher();
        await using var context = CreateContext(connection, publisher);
        await using var transaction = await context.Database.BeginTransactionAsync();

        context.Notifications.Add(NewNotification(Guid.NewGuid()));
        await context.SaveChangesAsync();
        await transaction.RollbackAsync();

        Assert.Empty(publisher.Events);
    }

    [PostgresFact]
    public async Task FailedPush_DoesNotFailTheSave()
    {
        await using var connection = await OpenTemporaryNotificationsAsync();
        var publisher = new RecordingPublisher { FailOnPublish = true };
        await using var context = CreateContext(connection, publisher);
        var notification = NewNotification(Guid.NewGuid());
        context.Notifications.Add(notification);

        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();
        Assert.True(await context.Notifications.AnyAsync(row => row.Id == notification.Id));
    }

    [PostgresFact]
    public async Task FailedCount_SkipsThePushAndKeepsTheCommittedChange()
    {
        await using var connection = await OpenTemporaryNotificationsAsync();
        var publisher = new RecordingPublisher();
        await using var context = CreateContext(connection, publisher, countingWorks: false);
        await using var transaction = await context.Database.BeginTransactionAsync();
        var notification = NewNotification(Guid.NewGuid());
        context.Notifications.Add(notification);

        await context.SaveChangesAsync();
        await transaction.CommitAsync();

        // The bell waits for the next push or a reconnect; the change itself is saved.
        Assert.Empty(publisher.Events);
        context.ChangeTracker.Clear();
        Assert.True(await context.Notifications.AnyAsync(row => row.Id == notification.Id));
    }

    private static Notification NewNotification(Guid userId) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Type = (int)NotificationTypeEnum.TaskStatusChanged,
        WorkProjectId = Guid.NewGuid(),
        EntityType = (int)NotificationEntityTypeEnum.WorkTask,
        EntityId = Guid.NewGuid(),
        Parameters = new NotificationParameters { TaskCode = "41", ToStatusId = 2 },
        CreatedAt = DateTime.UtcNow
    };

    // The count after the commit is read on a context of its own, as in the app; here it shares the
    // test connection, the only one that sees the temporary table.
    private static ProjectDbContext CreateContext(
        NpgsqlConnection connection,
        RecordingPublisher publisher,
        bool countingWorks = true)
    {
        var services = new ServiceCollection();
        if (countingWorks)
        {
            services.AddDbContext<ProjectDbContext>(options => options.UseNpgsql(connection));
        }

        var scopeFactory = services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>();
        return new ProjectDbContext(new DbContextOptionsBuilder<ProjectDbContext>()
            .UseNpgsql(connection)
            .AddInterceptors(new NotificationRealtimeInterceptor(
                publisher,
                scopeFactory,
                NullLogger<NotificationRealtimeInterceptor>.Instance))
            .Options);
    }

    private static async Task<NpgsqlConnection> OpenTemporaryNotificationsAsync()
    {
        var connectionString = Environment.GetEnvironmentVariable("ATMS_REALTIME_TEST_DB");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ATMS_REALTIME_TEST_DB is required.");
        }

        var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            "CREATE TEMP TABLE \"Notifications\" (LIKE public.\"Notifications\" INCLUDING DEFAULTS)", connection);
        await command.ExecuteNonQueryAsync();
        return connection;
    }

    private sealed class RecordingPublisher : IRealtimeEventPublisher
    {
        public bool FailOnPublish { get; init; }

        public List<(Guid UserId, string EventName, object Payload)> Events { get; } = [];

        public Task PublishToUsersAsync<T>(
            IEnumerable<Guid> userIds, string eventName, T payload, CancellationToken cancellationToken)
        {
            if (FailOnPublish)
            {
                throw new InvalidOperationException("Push failed");
            }

            Events.Add((Assert.Single(userIds), eventName, payload!));
            return Task.CompletedTask;
        }

        public Task PublishToProjectAsync<T>(
            Guid projectId, string eventName, T payload, CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task PublishToTaskAsync<T>(
            Guid taskId, string eventName, T payload, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }
}

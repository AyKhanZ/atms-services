using ATMS.Application.Realtime;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;

namespace Project.Services.Tests.Realtime;

public sealed class ProjectRealtimeInterceptorTest
{
    [PostgresFact]
    public async Task DatabaseSaveWithoutTransaction_PublishesAfterSave()
    {
        await using var connection = await OpenTemporaryTasksAsync();

        var publisher = new RecordingPublisher();
        await using var context = CreateConnectedContext(connection, publisher);
        var task = AddPersistableTask(context);

        await context.SaveChangesAsync();

        var sent = Assert.Single(publisher.Events);
        Assert.Equal(task.WorkProjectId, sent.ProjectId);
        Assert.Equal("created", sent.Action);
    }

    [PostgresFact]
    public async Task DatabaseTransactionRolledBack_PublishesNothing()
    {
        await using var connection = await OpenTemporaryTasksAsync();

        var publisher = new RecordingPublisher();
        await using var context = CreateConnectedContext(connection, publisher);
        await using var transaction = await context.Database.BeginTransactionAsync();
        AddPersistableTask(context);

        await context.SaveChangesAsync();
        Assert.Empty(publisher.Events);
        await transaction.RollbackAsync();

        Assert.Empty(publisher.Events);
    }

    [PostgresFact]
    public async Task DatabaseTwoSavesOfOneTask_PublishesOnceAfterCommit()
    {
        await using var connection = await OpenTemporaryTasksAsync();

        var publisher = new RecordingPublisher();
        await using var context = CreateConnectedContext(connection, publisher);
        await using var transaction = await context.Database.BeginTransactionAsync();
        var task = AddPersistableTask(context);

        await context.SaveChangesAsync();
        task.Title = "Renamed task";
        await context.SaveChangesAsync();
        Assert.Empty(publisher.Events);

        await transaction.CommitAsync();

        var sent = Assert.Single(publisher.Events);
        Assert.Equal(task.Id, sent.Id);
        Assert.Equal("created", sent.Action);
    }

    [PostgresFact]
    public async Task DatabaseAbandonedTransaction_DoesNotPublishWithNextTransaction()
    {
        await using var connection = await OpenTemporaryTasksAsync();

        var publisher = new RecordingPublisher();
        await using var context = CreateConnectedContext(connection, publisher);
        Guid abandonedTaskId;

        await using (var abandoned = await context.Database.BeginTransactionAsync())
        {
            var task = AddPersistableTask(context);
            abandonedTaskId = task.Id;
            await context.SaveChangesAsync();
        }

        await using var next = await context.Database.BeginTransactionAsync();
        var committedTask = AddPersistableTask(context);
        await context.SaveChangesAsync();
        Assert.Empty(publisher.Events);

        await next.CommitAsync();

        var sent = Assert.Single(publisher.Events);
        Assert.Equal(committedTask.Id, sent.Id);
        Assert.NotEqual(abandonedTaskId, sent.Id);
    }

    [Fact]
    public async Task SaveWithoutTransaction_PublishesOneTaskEventToItsProject()
    {
        await using var context = CreateContext();
        var task = AddTask(context);
        var publisher = new RecordingPublisher();
        var interceptor = CreateInterceptor(publisher);

        await CollectAsync(interceptor, context);
        await SaveWithoutTransactionAsync(interceptor, context);

        var sent = Assert.Single(publisher.Events);
        Assert.Equal(task.WorkProjectId, sent.ProjectId);
        Assert.Equal(task.Id, sent.Id);
        Assert.Equal("task", sent.EntityType);
        Assert.Equal("created", sent.Action);
        Assert.Equal(RealtimeEventNames.WorkItemChanged, publisher.EventNames.Single());
    }

    [Fact]
    public async Task UpdatedTask_PublishesOneUpdatedEventToItsProject()
    {
        await using var context = CreateContext();
        var task = AddTask(context);
        context.Entry(task).State = EntityState.Unchanged;
        task.Title = "Renamed task";
        var publisher = new RecordingPublisher();
        var interceptor = CreateInterceptor(publisher);

        await CollectAsync(interceptor, context);
        await SaveWithoutTransactionAsync(interceptor, context);

        var sent = Assert.Single(publisher.Events);
        Assert.Equal(task.WorkProjectId, sent.ProjectId);
        Assert.Equal(task.Id, sent.Id);
        Assert.Equal("task", sent.EntityType);
        Assert.Equal("updated", sent.Action);
    }

    [Fact]
    public async Task PublisherFails_SaveStillCompletes()
    {
        await using var context = CreateContext();
        AddTask(context);
        var publisher = new RecordingPublisher { FailOnPublish = true };
        var interceptor = CreateInterceptor(publisher);

        await CollectAsync(interceptor, context);
        await SaveWithoutTransactionAsync(interceptor, context);

        Assert.Empty(publisher.Events);
    }

    [Fact]
    public async Task TransactionRolledBack_PublishesNothing()
    {
        await using var context = CreateContext();
        AddTask(context);
        var publisher = new RecordingPublisher();
        var interceptor = CreateInterceptor(publisher);

        await CollectAsync(interceptor, context);
        await interceptor.SavedAsync(inTransaction: true);
        await interceptor.TransactionRolledBackAsync(null!, null!);

        Assert.Empty(publisher.Events);
    }

    [Fact]
    public async Task TransactionStartedAfterAbandonedTransaction_DiscardsPendingChanges()
    {
        await using var context = CreateContext();
        AddTask(context);
        var publisher = new RecordingPublisher();
        var interceptor = CreateInterceptor(publisher);

        await CollectAsync(interceptor, context);
        await interceptor.SavedAsync(inTransaction: true);
        await interceptor.TransactionStartedAsync(null!, null!, null!);
        await interceptor.TransactionCommittedAsync(null!, null!);

        Assert.Empty(publisher.Events);
    }

    [Fact]
    public async Task TwoSavesOfOneTaskInTransaction_PublishOnceAfterCommit()
    {
        await using var context = CreateContext();
        var task = AddTask(context);
        var publisher = new RecordingPublisher();
        var interceptor = CreateInterceptor(publisher);

        await CollectAsync(interceptor, context);
        await interceptor.SavedAsync(inTransaction: true);
        Assert.Empty(publisher.Events);

        context.Entry(task).State = EntityState.Unchanged;
        task.Title = "Updated";
        await CollectAsync(interceptor, context);
        await interceptor.SavedAsync(inTransaction: true);
        Assert.Empty(publisher.Events);

        await interceptor.TransactionCommittedAsync(null!, null!);

        var sent = Assert.Single(publisher.Events);
        Assert.Equal(task.Id, sent.Id);
        Assert.Equal("created", sent.Action);
    }

    [Fact]
    public async Task SaveFailed_DiscardsPendingChanges()
    {
        await using var context = CreateContext();
        AddTask(context);
        var publisher = new RecordingPublisher();
        var interceptor = CreateInterceptor(publisher);

        await CollectAsync(interceptor, context);
        await interceptor.SavedAsync(inTransaction: true);
        await interceptor.SaveChangesFailedAsync(null!);
        await interceptor.TransactionCommittedAsync(null!, null!);

        Assert.Empty(publisher.Events);
    }

    [Fact]
    public async Task SoftDeletedTicket_PublishesDeletedAction()
    {
        await using var context = CreateContext();
        var ticket = new WorkTicket { Id = Guid.NewGuid(), WorkProjectId = Guid.NewGuid() };
        context.WorkTickets.Attach(ticket);
        ticket.IsDeleted = true;
        var publisher = new RecordingPublisher();
        var interceptor = CreateInterceptor(publisher);

        await CollectAsync(interceptor, context);
        await SaveWithoutTransactionAsync(interceptor, context);

        var sent = Assert.Single(publisher.Events);
        Assert.Equal(ticket.WorkProjectId, sent.ProjectId);
        Assert.Equal("ticket", sent.EntityType);
        Assert.Equal("deleted", sent.Action);
    }

    private static ProjectDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ProjectDbContext>()
            .UseNpgsql("Host=localhost;Database=realtime_tests_not_connected")
            .Options;
        return new ProjectDbContext(options);
    }

    private static async Task<NpgsqlConnection> OpenTemporaryTasksAsync()
    {
        var connectionString = Environment.GetEnvironmentVariable("ATMS_REALTIME_TEST_DB");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ATMS_REALTIME_TEST_DB must be set to run the PostgreSQL transaction tests.");
        }

        var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            "CREATE TEMP TABLE \"Tasks\" (LIKE public.\"Tasks\" INCLUDING DEFAULTS)", connection);
        await command.ExecuteNonQueryAsync();
        return connection;
    }

    private static ProjectDbContext CreateConnectedContext(NpgsqlConnection connection, RecordingPublisher publisher)
    {
        var interceptor = CreateInterceptor(publisher);
        var options = new DbContextOptionsBuilder<ProjectDbContext>()
            .UseNpgsql(connection)
            .AddInterceptors(interceptor)
            .Options;
        return new ProjectDbContext(options);
    }

    private static WorkTask AddPersistableTask(ProjectDbContext context)
    {
        var task = new WorkTask
        {
            Id = Guid.NewGuid(),
            Code = $"rt-{Guid.NewGuid():N}",
            Title = "Realtime test task",
            StatusId = 1,
            PriorityId = 1,
            Rank = $"rt-{Guid.NewGuid():N}",
            WorkTicketId = Guid.NewGuid(),
            WorkProjectId = Guid.NewGuid(),
            CreatedById = Guid.NewGuid()
        };
        context.WorkTasks.Add(task);
        return task;
    }

    private static WorkTask AddTask(ProjectDbContext context)
    {
        var task = new WorkTask { Id = Guid.NewGuid(), WorkProjectId = Guid.NewGuid(), Title = "New task" };
        context.WorkTasks.Add(task);
        return task;
    }

    private static ProjectRealtimeInterceptor CreateInterceptor(RecordingPublisher publisher) =>
        new(publisher, NullLogger<ProjectRealtimeInterceptor>.Instance);

    // The interceptor reads only Context from save events and ignores transaction event arguments.
    private static Task CollectAsync(ProjectRealtimeInterceptor interceptor, ProjectDbContext context) =>
        interceptor.SavingChangesAsync(
            new DbContextEventData(null!, (_, _) => string.Empty, context),
            default).AsTask();

    private static Task SaveWithoutTransactionAsync(ProjectRealtimeInterceptor interceptor, ProjectDbContext context) =>
        interceptor.SavedChangesAsync(
            new SaveChangesCompletedEventData(null!, (_, _) => string.Empty, context, 1),
            1).AsTask();

    private sealed class RecordingPublisher : IRealtimeEventPublisher
    {
        public bool FailOnPublish { get; init; }

        public List<WorkItemChangedEvent> Events { get; } = [];
        public List<string> EventNames { get; } = [];

        public Task PublishToUsersAsync<T>(
            IEnumerable<Guid> userIds, string eventName, T payload, CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task PublishToTaskAsync<T>(
            Guid taskId, bool teamOnly, string eventName, T payload, CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task PublishToProjectAsync<T>(
            Guid projectId, string eventName, T payload, CancellationToken cancellationToken)
        {
            if (FailOnPublish)
            {
                throw new InvalidOperationException("Push failed");
            }

            Events.Add(Assert.IsType<WorkItemChangedEvent>(payload));
            EventNames.Add(eventName);
            return Task.CompletedTask;
        }
    }
}

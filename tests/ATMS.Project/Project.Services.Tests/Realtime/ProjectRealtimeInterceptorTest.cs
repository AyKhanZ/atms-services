using ATMS.Application.Realtime;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;

namespace Project.Services.Tests.Realtime;

public sealed class ProjectRealtimeInterceptorTest
{
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
            Guid taskId, string eventName, T payload, CancellationToken cancellationToken) =>
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

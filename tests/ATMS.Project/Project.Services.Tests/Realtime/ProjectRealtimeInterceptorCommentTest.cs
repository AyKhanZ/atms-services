using ATMS.Application.Realtime;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;

namespace Project.Services.Tests.Realtime;

public sealed class ProjectRealtimeInterceptorCommentTest
{
    [Fact]
    public async Task TextChange_PublishesOneUpdateToTask()
    {
        await using var context = CreateUnconnectedContext();
        var comment = new Comment { Id = Guid.NewGuid(), OwnerId = Guid.NewGuid(), Text = "Text" };
        context.Comments.Attach(comment);
        comment.Text = "Updated";
        var publisher = new RecordingPublisher();
        var interceptor = CreateInterceptor(publisher);

        await interceptor.SavingChangesAsync(Event(context), default);
        await interceptor.SavedChangesAsync(SavedEvent(context), 1);

        var sent = Assert.Single(publisher.Events);
        Assert.Equal(comment.OwnerId, sent.TaskId);
        Assert.Equal("updated", sent.Payload.Action);
    }

    [Fact]
    public async Task SoftDelete_PublishesDeletionToTask()
    {
        await using var context = CreateUnconnectedContext();
        var comment = new Comment { Id = Guid.NewGuid(), OwnerId = Guid.NewGuid(), Text = "Text" };
        context.Comments.Attach(comment);
        comment.IsDeleted = true;
        var publisher = new RecordingPublisher();
        var interceptor = CreateInterceptor(publisher);

        await interceptor.SavingChangesAsync(Event(context), default);
        await interceptor.SavedChangesAsync(SavedEvent(context), 1);

        var sent = Assert.Single(publisher.Events);
        Assert.Equal(comment.OwnerId, sent.TaskId);
        Assert.Equal("deleted", sent.Payload.Action);
    }

    private static ProjectRealtimeInterceptor CreateInterceptor(RecordingPublisher publisher) =>
        new(publisher, NullLogger<ProjectRealtimeInterceptor>.Instance);

    private static ProjectDbContext CreateUnconnectedContext() => new(
        new DbContextOptionsBuilder<ProjectDbContext>()
            .UseNpgsql("Host=localhost;Database=comment_realtime_tests_not_connected")
            .Options);

    private static DbContextEventData Event(ProjectDbContext context) =>
        new(null!, (_, _) => string.Empty, context);

    private static SaveChangesCompletedEventData SavedEvent(ProjectDbContext context) =>
        new(null!, (_, _) => string.Empty, context, 1);

    private sealed class RecordingPublisher : IRealtimeEventPublisher
    {
        public List<(Guid TaskId, CommentChangedEvent Payload)> Events { get; } = [];

        public Task PublishToUsersAsync<T>(
            IEnumerable<Guid> userIds, string eventName, T payload, CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task PublishToProjectAsync<T>(
            Guid projectId, string eventName, T payload, CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task PublishToTaskAsync<T>(
            Guid taskId, string eventName, T payload, CancellationToken cancellationToken)
        {
            Assert.Equal(RealtimeEventNames.CommentChanged, eventName);
            Events.Add((taskId, Assert.IsType<CommentChangedEvent>(payload)));
            return Task.CompletedTask;
        }
    }
}

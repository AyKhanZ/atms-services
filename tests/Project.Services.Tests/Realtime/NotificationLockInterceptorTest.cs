using ATMS.Application.Realtime;
using ATMS.Data.Enums;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Interceptors;
using ATMS.Project.Data.Repositories;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Models.Notifications;
using ATMS.Project.Services.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Npgsql;

namespace Project.Services.Tests.Realtime;

// Two changes of one task at the same moment, on two real connections: the second waits for the first
// and merges into what it wrote. A schema of its own lets both connections see the same table while
// the persistent one stays untouched.
public sealed class NotificationLockInterceptorTest : IAsyncLifetime
{
    private readonly string _schema = $"notification_lock_test_{Guid.NewGuid():N}";
    private string _connectionString = string.Empty;

    public async Task InitializeAsync()
    {
        _connectionString = Environment.GetEnvironmentVariable("ATMS_REALTIME_TEST_DB") ?? string.Empty;
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            return;
        }

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand($"""
            CREATE SCHEMA "{_schema}";
            CREATE TABLE "{_schema}"."Notifications" (LIKE public."Notifications" INCLUDING DEFAULTS INCLUDING INDEXES);
            """, connection);
        await command.ExecuteNonQueryAsync();
    }

    public async Task DisposeAsync()
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            return;
        }

        NpgsqlConnection.ClearAllPools();
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand($"""DROP SCHEMA "{_schema}" CASCADE""", connection);
        await command.ExecuteNonQueryAsync();
    }

    [PostgresFact]
    public async Task ChangesOfOneTaskAtOnce_TheSecondWaitsAndMergesIntoTheFirst()
    {
        var userId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var (first, firstService) = await OpenAsync();
        var (second, secondService) = await OpenAsync();
        await using (first.Context)
        await using (second.Context)
        {
            await firstService.AddAsync(Draft(projectId, "Alpha"), [userId], CancellationToken.None);

            var waiting = secondService.AddAsync(Draft(projectId, "Alpha renamed"), [userId], CancellationToken.None);
            await Task.Delay(500);
            Assert.False(waiting.IsCompleted);

            await first.Context.SaveChangesAsync();
            await waiting;
            await second.Context.SaveChangesAsync();
        }

        await using var check = (await OpenAsync()).Item1.Context;
        var row = Assert.Single(await check.Notifications.AsNoTracking().ToArrayAsync());
        Assert.Equal("Alpha renamed", row.Parameters.ProjectTitle);
    }

    [PostgresFact]
    public async Task ChangesOfDifferentTasks_DoNotWaitForEachOther()
    {
        var userId = Guid.NewGuid();
        var (first, firstService) = await OpenAsync();
        var (second, secondService) = await OpenAsync();
        await using (first.Context)
        await using (second.Context)
        {
            await firstService.AddAsync(Draft(Guid.NewGuid(), "Alpha"), [userId], CancellationToken.None);

            var other = secondService.AddAsync(Draft(Guid.NewGuid(), "Beta"), [userId], CancellationToken.None);
            Assert.True(await Task.WhenAny(other, Task.Delay(5000)) == other);

            await first.Context.SaveChangesAsync();
            await second.Context.SaveChangesAsync();
        }

        await using var check = (await OpenAsync()).Item1.Context;
        Assert.Equal(2, await check.Notifications.CountAsync());
    }

    [PostgresFact]
    public async Task TheTransactionItOpened_IsCommittedAfterTheSaveAndTheBellIsPushed()
    {
        var userId = Guid.NewGuid();
        var publisher = new Mock<IRealtimeEventPublisher>();
        var (session, service) = await OpenAsync(publisher.Object);
        await using (session.Context)
        {
            await service.AddAsync(Draft(Guid.NewGuid(), "Alpha"), [userId], CancellationToken.None);
            Assert.NotNull(session.Context.Database.CurrentTransaction);

            await session.Context.SaveChangesAsync();

            Assert.Null(session.Context.Database.CurrentTransaction);
        }

        // Pushed once the lock's transaction committed, not before.
        publisher.Verify(push => push.PublishToUsersAsync(
            It.Is<IEnumerable<Guid>>(userIds => userIds.Single() == userId),
            RealtimeEventNames.NotificationCreated,
            It.Is<NotificationCreatedEvent>(created => created.UnreadCount == 1),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [PostgresFact]
    public async Task ACallersOwnTransaction_IsLeftForTheCallerToCommit()
    {
        var (session, service) = await OpenAsync();
        await using (session.Context)
        {
            await using var transaction = await session.Context.Database.BeginTransactionAsync();
            await service.AddAsync(Draft(Guid.NewGuid(), "Alpha"), [Guid.NewGuid()], CancellationToken.None);

            await session.Context.SaveChangesAsync();

            Assert.Same(transaction, session.Context.Database.CurrentTransaction);
            await transaction.RollbackAsync();
        }

        await using var check = (await OpenAsync()).Item1.Context;
        Assert.Equal(0, await check.Notifications.CountAsync());
    }

    // With a publisher, the context also pushes the bell, in the order the app registers the two.
    private async Task<((ProjectDbContext Context, NpgsqlConnection Connection), NotificationService)> OpenAsync(
        IRealtimeEventPublisher? publisher = null)
    {
        var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await using (var command = new NpgsqlCommand($"""SET search_path TO "{_schema}", public""", connection))
        {
            await command.ExecuteNonQueryAsync();
        }

        var locks = new NotificationLockInterceptor();
        var options = new DbContextOptionsBuilder<ProjectDbContext>().UseNpgsql(connection);
        if (publisher is not null)
        {
            var counting = new ServiceCollection();
            counting.AddDbContext<ProjectDbContext>(count => count.UseNpgsql(connection));
            options.AddInterceptors(new NotificationRealtimeInterceptor(
                publisher,
                counting.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>(),
                NullLogger<NotificationRealtimeInterceptor>.Instance));
        }

        var context = new ProjectDbContext(options.AddInterceptors(locks).Options);
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["NotificationsOptions:AppUrl"] = "http://localhost:4200"
            })
            .Build();
        var service = new NotificationService(
            new NotificationRepository(context, locks),
            new Mock<IEmailDeliveryRepository>().Object,
            new Mock<IProjectPermissionRepository>().Object,
            configuration);
        return ((context, connection), service);
    }

    // AddedToProject skips the Project view check, so the test needs no participant tables.
    private static NotificationDraft Draft(Guid projectId, string projectTitle) =>
        new(
            NotificationTypeEnum.AddedToProject,
            projectId,
            NotificationEntityTypeEnum.Project,
            projectId,
            new NotificationParameters { ProjectTitle = projectTitle })
        {
            ActorId = Guid.NewGuid()
        };
}

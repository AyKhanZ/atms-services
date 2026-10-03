using ATMS.Data.Constants;
using ATMS.Data.Enums;
using ATMS.Project.Data.Interceptors;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Models.Notifications;
using ATMS.Project.Data.Repositories;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Time;
using ATMS.Project.Services.Models.Notifications;
using ATMS.Project.Services.Notifications;
using ATMS.Project.Services.Notifications.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Npgsql;
using Project.Services.Tests.Realtime;

namespace Project.Services.Tests.Notifications;

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

    [PostgresFact]
    public async Task RemindAsync_TwoPassesInARow_SendOneReminder()
    {
        await using var connection = await OpenTemporaryTablesAsync();
        var projectId = Guid.NewGuid();
        var assigneeUserId = Guid.NewGuid();
        var managerUserId = Guid.NewGuid();
        var dueTaskId = Guid.NewGuid();
        var lateTaskId = Guid.NewGuid();
        await SeedAsync(connection, projectId, assigneeUserId, managerUserId, dueTaskId, lateTaskId);

        await PassAsync(connection);
        await PassAsync(connection);

        await using var context = NewContext(connection);
        var rows = await context.Notifications
            .OrderBy(notification => notification.Type)
            .ThenBy(notification => notification.UserId)
            .Select(notification => new { notification.Type, notification.UserId, notification.EntityId })
            .ToArrayAsync();
        Assert.Equal(3, rows.Length);
        // DueToday and TaskOverdue go by email too: one email per notification, none for the second pass.
        Assert.Equal(3, await context.EmailDeliveries.CountAsync());
        Assert.Contains(rows, row => row.Type == (int)NotificationTypeEnum.DueToday && row.UserId == assigneeUserId && row.EntityId == dueTaskId);
        Assert.Contains(rows, row => row.Type == (int)NotificationTypeEnum.TaskOverdue && row.UserId == assigneeUserId && row.EntityId == lateTaskId);
        Assert.Contains(rows, row => row.Type == (int)NotificationTypeEnum.TaskOverdue && row.UserId == managerUserId && row.EntityId == lateTaskId);
    }

    private static async Task PassAsync(NpgsqlConnection connection)
    {
        await using var context = NewContext(connection);
        var repository = new NotificationRepository(context, new NotificationLockInterceptor());
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["NotificationsOptions:SendEmails"] = "true",
                ["NotificationsOptions:AppUrl"] = "http://localhost:4200"
            })
            .Build();
        var service = new DeadlineNotificationService(
            repository,
            new NotificationService(
                repository,
                new EmailDeliveryRepository(context),
                new ProjectPermissionRepository(context),
                configuration),
            Baku);

        await service.RemindAsync(Now, CancellationToken.None);
    }

    private static ProjectDbContext NewContext(NpgsqlConnection connection) =>
        new(new DbContextOptionsBuilder<ProjectDbContext>().UseNpgsql(connection).Options);

    private static async Task SeedAsync(
        NpgsqlConnection connection,
        Guid projectId,
        Guid assigneeUserId,
        Guid managerUserId,
        Guid dueTaskId,
        Guid lateTaskId)
    {
        var assigneeParticipantId = Guid.NewGuid();
        var managerParticipantId = Guid.NewGuid();
        var developerRoleId = Guid.NewGuid();
        await using var command = new NpgsqlCommand("""
            INSERT INTO "Projects" ("Id", "Title", "IsDeleted") VALUES (@projectId, 'Project Alpha', false);
            INSERT INTO "Roles" ("Id", "IsDeleted") VALUES (@developerRoleId, false), (@managerRoleId, false);
            INSERT INTO "RolePermissions" ("RoleId", "PermissionId") VALUES (@developerRoleId, 1), (@managerRoleId, 1);
            INSERT INTO "ProjectParticipants" ("Id", "UserId", "WorkProjectId", "IsDeleted") VALUES
                (@assigneeParticipantId, @assigneeUserId, @projectId, false),
                (@managerParticipantId, @managerUserId, @projectId, false);
            INSERT INTO "ProjectParticipantRoles" ("Id", "WorkProjectParticipantId", "RoleId", "IsDeleted") VALUES
                (gen_random_uuid(), @assigneeParticipantId, @developerRoleId, false),
                (gen_random_uuid(), @managerParticipantId, @managerRoleId, false);
            INSERT INTO "Tasks" ("Id", "WorkProjectId", "Code", "Title", "ParentWorkTaskId", "AssigneeId", "StatusId", "Deadline", "IsDeleted") VALUES
                (@dueTaskId, @projectId, '41', 'Due today', NULL, @assigneeParticipantId, 1, @today, false),
                (@lateTaskId, @projectId, '42', 'Late', NULL, @assigneeParticipantId, 2, @yesterday, false),
                (gen_random_uuid(), @projectId, '43', 'Done', NULL, @assigneeParticipantId, 3, @yesterday, false),
                (gen_random_uuid(), @projectId, '44', 'Deleted', NULL, @assigneeParticipantId, 1, @yesterday, true),
                (gen_random_uuid(), @projectId, '45', 'Long ago', NULL, @assigneeParticipantId, 1, @longAgo, false),
                (gen_random_uuid(), @projectId, '46', 'Tomorrow', NULL, @assigneeParticipantId, 1, @tomorrow, false);
            """, connection);
        command.Parameters.AddWithValue("projectId", projectId);
        command.Parameters.AddWithValue("developerRoleId", developerRoleId);
        command.Parameters.AddWithValue("managerRoleId", RoleIds.ProjectManager);
        command.Parameters.AddWithValue("assigneeParticipantId", assigneeParticipantId);
        command.Parameters.AddWithValue("managerParticipantId", managerParticipantId);
        command.Parameters.AddWithValue("assigneeUserId", assigneeUserId);
        command.Parameters.AddWithValue("managerUserId", managerUserId);
        command.Parameters.AddWithValue("dueTaskId", dueTaskId);
        command.Parameters.AddWithValue("lateTaskId", lateTaskId);
        command.Parameters.AddWithValue("today", StartOfToday);
        command.Parameters.AddWithValue("yesterday", StartOfToday.AddDays(-1));
        command.Parameters.AddWithValue("longAgo", StartOfToday.AddDays(-8));
        command.Parameters.AddWithValue("tomorrow", StartOfToday.AddDays(1));
        await command.ExecuteNonQueryAsync();
    }

    private static async Task<NpgsqlConnection> OpenTemporaryTablesAsync()
    {
        var connectionString = Environment.GetEnvironmentVariable("ATMS_REALTIME_TEST_DB");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ATMS_REALTIME_TEST_DB is required.");
        }

        var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand("""
            CREATE TEMP TABLE "Notifications" (LIKE public."Notifications" INCLUDING DEFAULTS INCLUDING INDEXES);
            CREATE TEMP TABLE "EmailDeliveries" (LIKE public."EmailDeliveries" INCLUDING DEFAULTS);
            CREATE TEMP TABLE "Projects" ("Id" uuid NOT NULL, "Title" text NOT NULL, "IsDeleted" boolean NOT NULL);
            CREATE TEMP TABLE "Roles" ("Id" uuid NOT NULL, "IsDeleted" boolean NOT NULL);
            CREATE TEMP TABLE "RolePermissions" ("RoleId" uuid NOT NULL, "PermissionId" integer NOT NULL);
            CREATE TEMP TABLE "ProjectParticipants" (
                "Id" uuid NOT NULL,
                "UserId" uuid NOT NULL,
                "WorkProjectId" uuid NOT NULL,
                "IsDeleted" boolean NOT NULL);
            CREATE TEMP TABLE "ProjectParticipantRoles" (
                "Id" uuid NOT NULL,
                "WorkProjectParticipantId" uuid NOT NULL,
                "RoleId" uuid NOT NULL,
                "IsDeleted" boolean NOT NULL);
            CREATE TEMP TABLE "Tasks" (
                "Id" uuid NOT NULL,
                "WorkProjectId" uuid NOT NULL,
                "Code" text NOT NULL,
                "Title" text NOT NULL,
                "ParentWorkTaskId" uuid NULL,
                "AssigneeId" uuid NULL,
                "StatusId" integer NOT NULL,
                "Deadline" timestamp with time zone NULL,
                "IsDeleted" boolean NOT NULL);
            """, connection);
        await command.ExecuteNonQueryAsync();
        return connection;
    }
}

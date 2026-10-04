using ATMS.Data.Criteria;
using ATMS.Data.Enums;
using ATMS.Project.Data.Criteria.Notifications;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Interceptors;
using ATMS.Project.Data.Models.Notifications;
using ATMS.Project.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Project.Services.Tests.Realtime;

namespace Project.Services.Tests.Repositories;

public sealed class NotificationRepositoryTest
{
    private static readonly Guid TaskId = Guid.NewGuid();

    [PostgresFact]
    public async Task GetManyAsync_ShowsOnlyTheCallersOwnNewestFirstWithWhatWasDeleted()
    {
        await using var connection = await OpenTemporaryWorkTablesAsync();
        await using var context = NewContext(connection);
        var userId = Guid.NewGuid();
        var actorId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var deletedProjectId = Guid.NewGuid();
        var liveTaskId = Guid.NewGuid();
        var deletedTaskId = Guid.NewGuid();
        var liveCommentId = Guid.NewGuid();
        var deletedCommentId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        await ExecuteAsync(connection, """
            INSERT INTO "Users" ("Id", "Name", "Surname", "AvatarPath", "IsDeleted")
            VALUES (@actorId, 'Leyla', 'Mammadova', 'users/leyla.png', true);
            INSERT INTO "Projects" ("Id", "Title", "IsDeleted") VALUES
                (@projectId, 'Project Alpha', false),
                (@deletedProjectId, 'Old project', true);
            INSERT INTO "Tasks" ("Id", "WorkProjectId", "Code", "Title", "WorkTicketId", "StatusId", "Deadline", "CreatedById", "IsDeleted") VALUES
                (@liveTaskId, @projectId, '41', 'Live', @ticketId, 2, '2026-09-30T20:00:00Z', @actorId, false),
                (@deletedTaskId, @projectId, '42', 'Deleted', @ticketId, 3, NULL, @actorId, true);
            INSERT INTO "Comments" ("Id", "OwnerType", "OwnerId", "CreatedById", "IsDeleted") VALUES
                (@liveCommentId, 2, @liveTaskId, @actorId, false),
                (@deletedCommentId, 2, @liveTaskId, @actorId, true);
            """,
            ("actorId", actorId),
            ("projectId", projectId),
            ("deletedProjectId", deletedProjectId),
            ("liveTaskId", liveTaskId),
            ("deletedTaskId", deletedTaskId),
            ("liveCommentId", liveCommentId),
            ("deletedCommentId", deletedCommentId),
            ("ticketId", ticketId));

        var liveComment = NewNotification(userId, NotificationTypeEnum.CommentAdded, liveTaskId, minutesAgo: 5);
        liveComment.ActorId = actorId;
        liveComment.CommentId = liveCommentId;
        var deletedComment = NewNotification(userId, NotificationTypeEnum.Mentioned, liveTaskId, minutesAgo: 4);
        deletedComment.CommentId = deletedCommentId;
        deletedComment.ReadAt = DateTime.UtcNow;
        var deletedTask = NewNotification(userId, NotificationTypeEnum.TaskAssigned, deletedTaskId, minutesAgo: 3);
        var liveProject = NewProjectNotification(userId, projectId, minutesAgo: 2);
        var deletedProject = NewProjectNotification(userId, deletedProjectId, minutesAgo: 1);
        var someoneElses = NewNotification(Guid.NewGuid(), NotificationTypeEnum.TaskAssigned, liveTaskId, minutesAgo: 0);
        context.Notifications.AddRange(liveComment, deletedComment, deletedTask, liveProject, deletedProject, someoneElses);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        var repository = new NotificationRepository(context, new NotificationLockInterceptor());

        var first = await repository.GetManyAsync(
            userId,
            new NotificationFilter(),
            new KeysetPaginationCriteria<NotificationRow>(null, 3, SortDirectionEnum.Desc),
            CancellationToken.None);
        var second = await repository.GetManyAsync(
            userId,
            new NotificationFilter(),
            new KeysetPaginationCriteria<NotificationRow>(first.NextCursor, 3, SortDirectionEnum.Desc),
            CancellationToken.None);
        var unread = await repository.GetManyAsync(
            userId,
            new NotificationFilter { UnreadOnly = true },
            new KeysetPaginationCriteria<NotificationRow>(null, 20, SortDirectionEnum.Desc),
            CancellationToken.None);

        Assert.Equal([deletedProject.Id, liveProject.Id, deletedTask.Id], first.Items.Select(row => row.Id));
        Assert.True(first.HasMore);
        Assert.Equal([deletedComment.Id, liveComment.Id], second.Items.Select(row => row.Id));
        Assert.False(second.HasMore);
        Assert.DoesNotContain(deletedComment.Id, unread.Items.Select(row => row.Id));
        Assert.Equal(4, unread.Items.Count());

        var rows = first.Items.Concat(second.Items).ToDictionary(row => row.Id);
        Assert.True(rows[deletedProject.Id].EntityDeleted);
        Assert.False(rows[liveProject.Id].EntityDeleted);
        Assert.True(rows[deletedTask.Id].EntityDeleted);
        Assert.False(rows[liveComment.Id].EntityDeleted);
        Assert.False(rows[liveComment.Id].CommentDeleted);
        Assert.True(rows[deletedComment.Id].CommentDeleted);
        Assert.False(rows[deletedTask.Id].CommentDeleted);
        // A deleted actor keeps their name on what they did.
        Assert.Equal("Leyla", rows[liveComment.Id].Actor?.Name);
        Assert.Equal("users/leyla.png", rows[liveComment.Id].Actor?.AvatarPath);
        Assert.Null(rows[deletedTask.Id].Actor);
        Assert.Equal(ticketId, rows[liveComment.Id].WorkTicketId);
        Assert.Null(rows[deletedTask.Id].WorkTicketId);
        Assert.Null(rows[liveProject.Id].WorkTicketId);
        // Where the task stands now, for the overdue mark; nothing for a deleted task or a project.
        Assert.Equal((int)WorkTaskStatusEnum.InProgress, rows[liveComment.Id].TaskStatusId);
        Assert.Equal(new DateTime(2026, 9, 30, 20, 0, 0, DateTimeKind.Utc), rows[liveComment.Id].TaskDeadline);
        Assert.Null(rows[deletedTask.Id].TaskStatusId);
        Assert.Null(rows[liveProject.Id].TaskDeadline);
        Assert.Equal("Payment form", rows[liveComment.Id].Parameters.TaskTitle);
    }

    [PostgresFact]
    public async Task DeleteOldAsync_DropsReadAfterThirtyDaysAndAnyAfterNinetyInBatches()
    {
        await using var connection = await OpenTemporaryNotificationsAsync();
        await using var context = NewContext(connection);
        var userId = Guid.NewGuid();
        var minutesInDay = 24 * 60;
        var oldRead = Enumerable.Range(0, 5)
            .Select(_ => NewNotification(userId, NotificationTypeEnum.TaskAssigned, TaskId, minutesAgo: 31 * minutesInDay))
            .ToArray();
        foreach (var notification in oldRead)
        {
            notification.ReadAt = DateTime.UtcNow.AddDays(-31);
        }

        var oldUnread = NewNotification(userId, NotificationTypeEnum.TaskAssigned, TaskId, minutesAgo: 31 * minutesInDay);
        var ancientUnread = NewNotification(userId, NotificationTypeEnum.TaskAssigned, TaskId, minutesAgo: 91 * minutesInDay);
        var recentRead = NewNotification(userId, NotificationTypeEnum.TaskAssigned, TaskId, minutesAgo: 60);
        recentRead.ReadAt = DateTime.UtcNow;
        context.Notifications.AddRange([.. oldRead, oldUnread, ancientUnread, recentRead]);
        await context.SaveChangesAsync();
        var now = DateTime.UtcNow;

        // Batches of two: five old read and one ancient need four rounds, the last one short.
        var deleted = await new NotificationRepository(context, new NotificationLockInterceptor()).DeleteOldAsync(
            now.AddDays(-30), now.AddDays(-90), 2, CancellationToken.None);

        Assert.Equal(6, deleted);
        Assert.Equal(
            new[] { oldUnread.Id, recentRead.Id }.Order(),
            (await context.Notifications.AsNoTracking().Select(row => row.Id).ToArrayAsync()).Order());
    }

    [PostgresFact]
    public async Task OwnReads_NeverReachAnotherPersonsNotifications()
    {
        await using var connection = await OpenTemporaryNotificationsAsync();
        await using var context = NewContext(connection);
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var own = NewNotification(userId, NotificationTypeEnum.TaskAssigned, TaskId, minutesAgo: 1);
        var ownRead = NewNotification(userId, NotificationTypeEnum.TaskAssigned, TaskId, minutesAgo: 2);
        ownRead.ReadAt = DateTime.UtcNow;
        var others = NewNotification(otherUserId, NotificationTypeEnum.TaskAssigned, TaskId, minutesAgo: 1);
        context.Notifications.AddRange(own, ownRead, others);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        var repository = new NotificationRepository(context, new NotificationLockInterceptor());

        Assert.Equal(own.Id, (await repository.FindAsync(userId, own.Id, CancellationToken.None))?.Id);
        Assert.Null(await repository.FindAsync(userId, others.Id, CancellationToken.None));
        Assert.True(await repository.IsExistAsync(userId, own.Id, CancellationToken.None));
        Assert.False(await repository.IsExistAsync(userId, others.Id, CancellationToken.None));
        Assert.Equal(1, await repository.CountUnreadAsync(userId, CancellationToken.None));
        Assert.Equal(1, await repository.MarkAllReadAsync(userId, DateTime.UtcNow, CancellationToken.None));
        Assert.Equal(0, await repository.CountUnreadAsync(userId, CancellationToken.None));
        Assert.Equal(1, await repository.CountUnreadAsync(otherUserId, CancellationToken.None));
    }

    [PostgresFact]
    public async Task GetUnreadSinceAsync_FindsUnreadOfThesePeopleAndEntitiesAfterTheMoment()
    {
        await using var connection = await OpenTemporaryNotificationsAsync();
        await using var context = NewContext(connection);
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var since = DateTime.UtcNow.AddMinutes(-10);
        var match = NewNotification(userId, NotificationTypeEnum.TaskStatusChanged, TaskId, minutesAgo: 3);
        // Another kind is returned too: the caller matches the kind for many drafts at once.
        var otherKind = NewNotification(userId, NotificationTypeEnum.CommentAdded, TaskId, minutesAgo: 1);
        var read = NewNotification(userId, NotificationTypeEnum.TaskStatusChanged, TaskId, minutesAgo: 2);
        read.ReadAt = DateTime.UtcNow;
        context.Notifications.AddRange(
            match,
            otherKind,
            read,
            NewNotification(userId, NotificationTypeEnum.TaskStatusChanged, TaskId, minutesAgo: 11),
            NewNotification(userId, NotificationTypeEnum.TaskStatusChanged, Guid.NewGuid(), minutesAgo: 1),
            NewNotification(otherUserId, NotificationTypeEnum.TaskStatusChanged, TaskId, minutesAgo: 1));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var found = await new NotificationRepository(context, new NotificationLockInterceptor()).GetUnreadSinceAsync(
            [userId],
            [TaskId],
            since,
            CancellationToken.None);

        Assert.Equal(new[] { match.Id, otherKind.Id }.Order(), found.Select(row => row.Id).Order());
        Assert.Equal(match.Parameters, found.Single(row => row.Id == match.Id).Parameters);
    }

    [PostgresFact]
    public async Task GetUnreadSinceAsync_ReturnsTrackedRowsSoAMergeIsSaved()
    {
        await using var connection = await OpenTemporaryNotificationsAsync();
        await using var context = NewContext(connection);
        var userId = Guid.NewGuid();
        var notification = NewNotification(userId, NotificationTypeEnum.TaskStatusChanged, TaskId, minutesAgo: 1);
        context.Notifications.Add(notification);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var found = Assert.Single(await new NotificationRepository(context, new NotificationLockInterceptor()).GetUnreadSinceAsync(
            [userId],
            [TaskId],
            DateTime.UtcNow.AddMinutes(-10),
            CancellationToken.None));
        found.Parameters = found.Parameters with { ToStatusId = 4 };
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var saved = await context.Notifications.SingleAsync(row => row.Id == notification.Id);
        Assert.Equal(4, saved.Parameters.ToStatusId);
        Assert.Equal("Payment form", saved.Parameters.TaskTitle);
    }

    [PostgresFact]
    public async Task GetDedupKeysAsync_ReturnsOnlyThePeopleWhoHaveTheseKeys()
    {
        await using var connection = await OpenTemporaryNotificationsAsync();
        await using var context = NewContext(connection);
        var reminded = Guid.NewGuid();
        var notReminded = Guid.NewGuid();
        var withKey = NewNotification(reminded, NotificationTypeEnum.DueToday, TaskId, minutesAgo: 60);
        withKey.DedupKey = "due-today:key";
        var otherKey = NewNotification(notReminded, NotificationTypeEnum.DueToday, TaskId, minutesAgo: 60);
        otherKey.DedupKey = "due-today:other";
        context.Notifications.AddRange(withKey, otherKey);
        await context.SaveChangesAsync();

        var keys = await new NotificationRepository(context, new NotificationLockInterceptor()).GetDedupKeysAsync(
            [reminded, notReminded],
            ["due-today:key", "due-today:unknown"],
            CancellationToken.None);

        Assert.Equal([new NotificationKeyRow(reminded, "due-today:key")], keys);
    }

    [PostgresFact]
    public async Task DedupKey_IsUniquePerPerson()
    {
        await using var connection = await OpenTemporaryNotificationsAsync();
        await using var context = NewContext(connection);
        var userId = Guid.NewGuid();
        var first = NewNotification(userId, NotificationTypeEnum.DueToday, TaskId, minutesAgo: 1);
        first.DedupKey = "due-today:key";
        var second = NewNotification(userId, NotificationTypeEnum.DueToday, TaskId, minutesAgo: 1);
        second.DedupKey = "due-today:key";
        var otherPerson = NewNotification(Guid.NewGuid(), NotificationTypeEnum.DueToday, TaskId, minutesAgo: 1);
        otherPerson.DedupKey = "due-today:key";
        context.Notifications.AddRange(first, otherPerson);
        await context.SaveChangesAsync();

        context.Notifications.Add(second);

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [PostgresFact]
    public async Task GetWorkTaskAudienceAsync_ResolvesTheAssigneeParticipantToTheirUser()
    {
        await using var connection = await OpenTemporaryWorkTablesAsync();
        await using var context = NewContext(connection);
        var projectId = Guid.NewGuid();
        var participantId = Guid.NewGuid();
        var removedParticipantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        await ExecuteAsync(connection, """
            INSERT INTO "Projects" ("Id", "Title", "IsDeleted") VALUES (@projectId, 'Project Alpha', false);
            INSERT INTO "ProjectParticipants" ("Id", "UserId", "WorkProjectId", "IsDeleted") VALUES
                (@participantId, @userId, @projectId, false),
                (@removedParticipantId, @otherUserId, @projectId, true);
            """,
            ("projectId", projectId),
            ("participantId", participantId),
            ("removedParticipantId", removedParticipantId),
            ("userId", userId),
            ("otherUserId", Guid.NewGuid()));
        var repository = new NotificationRepository(context, new NotificationLockInterceptor());

        var assigned = await repository.GetWorkTaskAudienceAsync(projectId, participantId, CancellationToken.None);
        var unassigned = await repository.GetWorkTaskAudienceAsync(projectId, null, CancellationToken.None);
        var removed = await repository.GetWorkTaskAudienceAsync(projectId, removedParticipantId, CancellationToken.None);
        var missing = await repository.GetWorkTaskAudienceAsync(Guid.NewGuid(), participantId, CancellationToken.None);

        Assert.Equal(new WorkTaskAudienceRow("Project Alpha", userId), assigned);
        Assert.Equal(new WorkTaskAudienceRow("Project Alpha", null), unassigned);
        Assert.Equal(new WorkTaskAudienceRow("Project Alpha", null), removed);
        Assert.Null(missing);
    }

    [PostgresFact]
    public async Task GetCommentedTaskAsync_ReadsTheTaskItsPeopleAndLiveCommenters()
    {
        await using var connection = await OpenTemporaryWorkTablesAsync();
        await using var context = NewContext(connection);
        var projectId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var participantId = Guid.NewGuid();
        var assigneeUserId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var commenterId = Guid.NewGuid();
        var deletedCommenterId = Guid.NewGuid();
        await ExecuteAsync(connection, """
            INSERT INTO "Projects" ("Id", "Title", "IsDeleted") VALUES (@projectId, 'Project Alpha', false);
            INSERT INTO "ProjectParticipants" ("Id", "UserId", "WorkProjectId", "IsDeleted")
            VALUES (@participantId, @assigneeUserId, @projectId, false);
            INSERT INTO "Tasks" ("Id", "WorkProjectId", "Code", "Title", "ParentWorkTaskId", "AssigneeId", "CreatedById", "IsDeleted")
            VALUES (@taskId, @projectId, '41', 'Payment form', @parentId, @participantId, @authorId, false);
            INSERT INTO "Comments" ("Id", "OwnerType", "OwnerId", "CreatedById", "IsDeleted") VALUES
                (gen_random_uuid(), 2, @taskId, @commenterId, false),
                (gen_random_uuid(), 2, @taskId, @commenterId, false),
                (gen_random_uuid(), 2, @taskId, @deletedCommenterId, true),
                (gen_random_uuid(), 2, @otherTaskId, @otherCommenterId, false);
            """,
            ("projectId", projectId),
            ("taskId", taskId),
            ("parentId", Guid.NewGuid()),
            ("participantId", participantId),
            ("assigneeUserId", assigneeUserId),
            ("authorId", authorId),
            ("commenterId", commenterId),
            ("deletedCommenterId", deletedCommenterId),
            ("otherTaskId", Guid.NewGuid()),
            ("otherCommenterId", Guid.NewGuid()));
        var repository = new NotificationRepository(context, new NotificationLockInterceptor());

        var task = await repository.GetCommentedTaskAsync(projectId, taskId, CancellationToken.None);
        var fromOtherProject = await repository.GetCommentedTaskAsync(Guid.NewGuid(), taskId, CancellationToken.None);

        Assert.NotNull(task);
        Assert.Equal("Project Alpha", task.ProjectTitle);
        Assert.Equal("41", task.Code);
        Assert.Equal("Payment form", task.Title);
        Assert.True(task.IsSubtask);
        Assert.Equal(assigneeUserId, task.AssigneeUserId);
        Assert.Equal(authorId, task.AuthorId);
        Assert.Equal([commenterId], task.CommenterIds);
        Assert.Null(fromOtherProject);
    }

    private static Notification NewProjectNotification(Guid userId, Guid projectId, int minutesAgo)
    {
        var notification = NewNotification(userId, NotificationTypeEnum.AddedToProject, projectId, minutesAgo);
        notification.EntityType = (int)NotificationEntityTypeEnum.Project;
        return notification;
    }

    private static ProjectDbContext NewContext(NpgsqlConnection connection) =>
        new(new DbContextOptionsBuilder<ProjectDbContext>().UseNpgsql(connection).Options);

    private static Notification NewNotification(
        Guid userId,
        NotificationTypeEnum type,
        Guid entityId,
        int minutesAgo) =>
        new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = (int)type,
            WorkProjectId = Guid.NewGuid(),
            EntityType = (int)NotificationEntityTypeEnum.WorkTask,
            EntityId = entityId,
            Parameters = new NotificationParameters
            {
                ProjectTitle = "Project Alpha",
                TaskCode = "41",
                TaskTitle = "Payment form",
                TaskKind = (int)WorkTaskKindEnum.Task,
                FromStatusId = 1,
                ToStatusId = 2,
                Deadline = new DateOnly(2026, 10, 6)
            },
            CreatedAt = DateTime.UtcNow.AddMinutes(-minutesAgo)
        };

    private static async Task<NpgsqlConnection> OpenTemporaryWorkTablesAsync()
    {
        var connection = await OpenTemporaryNotificationsAsync();
        await ExecuteAsync(connection, """
            CREATE TEMP TABLE "Projects" ("Id" uuid NOT NULL, "Title" text NOT NULL, "IsDeleted" boolean NOT NULL);
            CREATE TEMP TABLE "Users" (
                "Id" uuid NOT NULL,
                "Name" text NOT NULL,
                "Surname" text NOT NULL,
                "AvatarPath" text NULL,
                "IsDeleted" boolean NOT NULL);
            CREATE TEMP TABLE "ProjectParticipants" (
                "Id" uuid NOT NULL,
                "UserId" uuid NOT NULL,
                "WorkProjectId" uuid NOT NULL,
                "IsDeleted" boolean NOT NULL);
            CREATE TEMP TABLE "Tasks" (
                "Id" uuid NOT NULL,
                "WorkProjectId" uuid NOT NULL,
                "Code" text NOT NULL,
                "Title" text NOT NULL,
                "ParentWorkTaskId" uuid NULL,
                "WorkTicketId" uuid NULL,
                "AssigneeId" uuid NULL,
                "StatusId" integer NOT NULL DEFAULT 1,
                "Deadline" timestamp with time zone NULL,
                "CreatedById" uuid NOT NULL,
                "IsDeleted" boolean NOT NULL);
            CREATE TEMP TABLE "Comments" (
                "Id" uuid NOT NULL,
                "OwnerType" integer NOT NULL,
                "OwnerId" uuid NOT NULL,
                "CreatedById" uuid NOT NULL,
                "IsDeleted" boolean NOT NULL);
            """);
        return connection;
    }

    private static async Task ExecuteAsync(
        NpgsqlConnection connection,
        string sql,
        params (string Name, object Value)[] parameters)
    {
        await using var command = new NpgsqlCommand(sql, connection);
        foreach (var (name, value) in parameters)
        {
            command.Parameters.AddWithValue(name, value);
        }

        await command.ExecuteNonQueryAsync();
    }

    // A temporary copy keeps the persistent table free of test rows; the unique index is copied
    // too, because one test is about it.
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
            """
            CREATE TEMP TABLE "Notifications" (LIKE public."Notifications" INCLUDING DEFAULTS INCLUDING INDEXES);
            """, connection);
        await command.ExecuteNonQueryAsync();
        return connection;
    }
}

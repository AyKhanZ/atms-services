using ATMS.Data.Constants;
using ATMS.Data.Criteria;
using ATMS.Data.Enums;
using ATMS.Project.Data.Criteria.Comments;
using ATMS.Project.Data.Criteria.WorkProjects;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Project.Services.Tests.Realtime;

namespace Project.Services.Tests.Repositories;

public sealed class CommentRepositoryTest
{
    [PostgresFact]
    public async Task Page_ShowsLiveCommentsOfTheTaskNewestFirstPageByPage()
    {
        await using var connection = await OpenTemporaryCommentsAsync();
        await using var context = new ProjectDbContext(new DbContextOptionsBuilder<ProjectDbContext>()
            .UseNpgsql(connection).Options);
        var projectId = Guid.NewGuid();
        var otherProjectId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        await AddTaskScopesAsync(connection, projectId, otherProjectId, taskId);
        var authorId = Guid.NewGuid();
        var comments = Enumerable.Range(1, 3)
            .Select(number => NewComment(taskId, authorId, $"Comment {number}"))
            .ToArray();
        var deleted = NewComment(taskId, authorId, "Deleted");
        deleted.IsDeleted = true;
        context.Comments.AddRange([.. comments, deleted]);
        await context.SaveChangesAsync();

        for (var index = 0; index < comments.Length; index++)
        {
            comments[index].CreatedAt = DateTime.UtcNow.AddMinutes(index);
        }
        deleted.CreatedAt = DateTime.UtcNow.AddMinutes(10);
        await context.SaveChangesAsync();

        var repository = new CommentRepository(context);
        var criteria = new CommentFilter { WorkTaskId = taskId };
        var first = await repository.GetManyAsync(
            projectId, criteria,
            new KeysetPaginationCriteria<Comment>(null, 2, SortDirectionEnum.Desc), CancellationToken.None);
        var second = await repository.GetManyAsync(
            projectId, criteria,
            new KeysetPaginationCriteria<Comment>(first.NextCursor, 2, SortDirectionEnum.Desc), CancellationToken.None);
        var otherProjectPage = await repository.GetManyAsync(
            otherProjectId, criteria,
            new KeysetPaginationCriteria<Comment>(null, 20, SortDirectionEnum.Desc), CancellationToken.None);

        Assert.Equal([comments[2].Id, comments[1].Id], first.Items.Select(comment => comment.Id));
        Assert.True(first.HasMore);
        Assert.Equal([comments[0].Id], second.Items.Select(comment => comment.Id));
        Assert.False(second.HasMore);
        Assert.Empty(otherProjectPage.Items);
        Assert.Equal(3, await repository.CountAsync(taskId, CancellationToken.None));
        Assert.NotNull(await repository.GetAsync(projectId, comments[0].Id, CancellationToken.None));
        Assert.Null(await repository.GetAsync(projectId, deleted.Id, CancellationToken.None));
        Assert.Null(await repository.GetAsync(otherProjectId, comments[0].Id, CancellationToken.None));
        Assert.Equal(authorId, await repository.GetAuthorIdAsync(projectId, comments[0].Id, CancellationToken.None));
    }

    // Read-only against the real schema: the lookup is a UNION ALL of tasks and tickets filtered by
    // project access, which only PostgreSQL can prove translates.
    [PostgresFact]
    public async Task References_FindTasksOnlyInReadableProjects()
    {
        await using var context = new ProjectDbContext(new DbContextOptionsBuilder<ProjectDbContext>()
            .UseNpgsql(Environment.GetEnvironmentVariable("ATMS_REALTIME_TEST_DB")).Options);
        var repository = new CommentRepository(context);
        var code = await context.WorkTasks.Select(task => task.Code).FirstOrDefaultAsync() ?? "999999999";

        var outsider = await repository.GetReferencesAsync(
            [code], new AccessibleWorkProjectsCriteria(Guid.NewGuid(), RoleIds.Employee), CancellationToken.None);
        var administrator = await repository.GetReferencesAsync(
            [code], new AccessibleWorkProjectsCriteria(Guid.NewGuid(), RoleIds.SuperAdmin), CancellationToken.None);

        Assert.Empty(outsider);
        Assert.All(administrator, row => Assert.Equal(code, row.Code));
    }

    private static Comment NewComment(Guid taskId, Guid authorId, string text) => new()
    {
        Id = Guid.NewGuid(),
        OwnerType = CommentOwnerTypeEnum.Task,
        OwnerId = taskId,
        Text = text,
        CreatedById = authorId
    };

    private static async Task<NpgsqlConnection> OpenTemporaryCommentsAsync()
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
            CREATE TEMP TABLE "Comments" (LIKE public."Comments" INCLUDING DEFAULTS);
            CREATE TEMP TABLE "Projects" ("Id" uuid NOT NULL, "IsDeleted" boolean NOT NULL);
            CREATE TEMP TABLE "Tickets" ("Id" uuid NOT NULL, "IsDeleted" boolean NOT NULL);
            CREATE TEMP TABLE "Tasks" (
                "Id" uuid NOT NULL,
                "WorkProjectId" uuid NOT NULL,
                "WorkTicketId" uuid NOT NULL,
                "IsDeleted" boolean NOT NULL);
            """, connection);
        await command.ExecuteNonQueryAsync();
        return connection;
    }

    private static async Task AddTaskScopesAsync(
        NpgsqlConnection connection, Guid projectId, Guid otherProjectId, Guid taskId)
    {
        var ticketId = Guid.NewGuid();
        await using var command = new NpgsqlCommand("""
            INSERT INTO "Projects" ("Id", "IsDeleted") VALUES (@projectId, false), (@otherProjectId, false);
            INSERT INTO "Tickets" ("Id", "IsDeleted") VALUES (@ticketId, false);
            INSERT INTO "Tasks" ("Id", "WorkProjectId", "WorkTicketId", "IsDeleted")
            VALUES (@taskId, @projectId, @ticketId, false);
            """, connection);
        command.Parameters.AddWithValue("projectId", projectId);
        command.Parameters.AddWithValue("otherProjectId", otherProjectId);
        command.Parameters.AddWithValue("ticketId", ticketId);
        command.Parameters.AddWithValue("taskId", taskId);
        await command.ExecuteNonQueryAsync();
    }
}

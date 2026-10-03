using ATMS.Data.Enums;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Models.WorkProjects;
using ATMS.Project.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Project.Services.Tests.Realtime;

namespace Project.Services.Tests.Repositories;

public sealed class ProjectPermissionRepositoryTest
{
    [PostgresFact]
    public async Task GetUsersWithPermissionAsync_KeepsOnlyLiveParticipantsWhoseRoleHasThePermission()
    {
        await using var connection = await OpenTemporaryParticipantsAsync();
        await using var context = new ProjectDbContext(new DbContextOptionsBuilder<ProjectDbContext>()
            .UseNpgsql(connection).Options);
        var projectId = Guid.NewGuid();
        var otherProjectId = Guid.NewGuid();
        var viewerRole = Guid.NewGuid();
        var otherRole = Guid.NewGuid();
        var deletedRole = Guid.NewGuid();
        await AddRoleAsync(connection, viewerRole, isDeleted: false, ProjectPermissionEnum.ProjectView, ProjectPermissionEnum.TaskEdit);
        await AddRoleAsync(connection, otherRole, isDeleted: false, ProjectPermissionEnum.TaskEdit);
        await AddRoleAsync(connection, deletedRole, isDeleted: true, ProjectPermissionEnum.ProjectView);

        var viewer = Guid.NewGuid();
        var viewerWithTwoRoles = Guid.NewGuid();
        var withoutView = Guid.NewGuid();
        var removedParticipant = Guid.NewGuid();
        var removedRole = Guid.NewGuid();
        var deletedRoleHolder = Guid.NewGuid();
        var inOtherProject = Guid.NewGuid();
        var notAsked = Guid.NewGuid();
        await AddParticipantAsync(connection, projectId, viewer, false, (viewerRole, false));
        await AddParticipantAsync(connection, projectId, viewerWithTwoRoles, false, (viewerRole, false), (otherRole, false));
        await AddParticipantAsync(connection, projectId, withoutView, false, (otherRole, false));
        await AddParticipantAsync(connection, projectId, removedParticipant, true, (viewerRole, false));
        await AddParticipantAsync(connection, projectId, removedRole, false, (viewerRole, true));
        await AddParticipantAsync(connection, projectId, deletedRoleHolder, false, (deletedRole, false));
        await AddParticipantAsync(connection, otherProjectId, inOtherProject, false, (viewerRole, false));
        await AddParticipantAsync(connection, projectId, notAsked, false, (viewerRole, false));

        var repository = new ProjectPermissionRepository(context);
        Guid[] asked = [viewer, viewerWithTwoRoles, withoutView, removedParticipant, removedRole, deletedRoleHolder, inOtherProject];

        var oneProject = await repository.GetUsersWithPermissionAsync(
            [projectId],
            asked,
            ProjectPermissionEnum.ProjectView,
            CancellationToken.None);
        var bothProjects = await repository.GetUsersWithPermissionAsync(
            [projectId, otherProjectId],
            asked,
            ProjectPermissionEnum.ProjectView,
            CancellationToken.None);

        Assert.Equal(
            new[] { new ProjectUserRow(projectId, viewer), new ProjectUserRow(projectId, viewerWithTwoRoles) }
                .OrderBy(row => row.UserId),
            oneProject.OrderBy(row => row.UserId));
        // Each person comes with the project they can see: the caller matches the pair, not the person.
        Assert.Contains(new ProjectUserRow(otherProjectId, inOtherProject), bothProjects);
        Assert.DoesNotContain(new ProjectUserRow(projectId, inOtherProject), bothProjects);
        Assert.Equal(3, bothProjects.Length);
    }

    private static async Task<NpgsqlConnection> OpenTemporaryParticipantsAsync()
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
            CREATE TEMP TABLE "Roles" ("Id" uuid NOT NULL, "IsDeleted" boolean NOT NULL);
            CREATE TEMP TABLE "RolePermissions" ("RoleId" uuid NOT NULL, "PermissionId" integer NOT NULL);
            """, connection);
        await command.ExecuteNonQueryAsync();
        return connection;
    }

    private static async Task AddRoleAsync(
        NpgsqlConnection connection,
        Guid roleId,
        bool isDeleted,
        params ProjectPermissionEnum[] permissions)
    {
        await using var role = new NpgsqlCommand(
            """INSERT INTO "Roles" ("Id", "IsDeleted") VALUES (@roleId, @isDeleted)""", connection);
        role.Parameters.AddWithValue("roleId", roleId);
        role.Parameters.AddWithValue("isDeleted", isDeleted);
        await role.ExecuteNonQueryAsync();

        foreach (var permission in permissions)
        {
            await using var rolePermission = new NpgsqlCommand(
                """INSERT INTO "RolePermissions" ("RoleId", "PermissionId") VALUES (@roleId, @permissionId)""",
                connection);
            rolePermission.Parameters.AddWithValue("roleId", roleId);
            rolePermission.Parameters.AddWithValue("permissionId", (int)permission);
            await rolePermission.ExecuteNonQueryAsync();
        }
    }

    private static async Task AddParticipantAsync(
        NpgsqlConnection connection,
        Guid projectId,
        Guid userId,
        bool isDeleted,
        params (Guid RoleId, bool IsDeleted)[] roles)
    {
        var participantId = Guid.NewGuid();
        await using var participant = new NpgsqlCommand(
            """
            INSERT INTO "ProjectParticipants" ("Id", "UserId", "WorkProjectId", "IsDeleted")
            VALUES (@participantId, @userId, @projectId, @isDeleted)
            """, connection);
        participant.Parameters.AddWithValue("participantId", participantId);
        participant.Parameters.AddWithValue("userId", userId);
        participant.Parameters.AddWithValue("projectId", projectId);
        participant.Parameters.AddWithValue("isDeleted", isDeleted);
        await participant.ExecuteNonQueryAsync();

        foreach (var role in roles)
        {
            await using var participantRole = new NpgsqlCommand(
                """
                INSERT INTO "ProjectParticipantRoles" ("Id", "WorkProjectParticipantId", "RoleId", "IsDeleted")
                VALUES (@id, @participantId, @roleId, @isDeleted)
                """, connection);
            participantRole.Parameters.AddWithValue("id", Guid.NewGuid());
            participantRole.Parameters.AddWithValue("participantId", participantId);
            participantRole.Parameters.AddWithValue("roleId", role.RoleId);
            participantRole.Parameters.AddWithValue("isDeleted", role.IsDeleted);
            await participantRole.ExecuteNonQueryAsync();
        }
    }
}

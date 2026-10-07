using ATMS.Data.Constants;
using ATMS.Data.Enums;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Enums;
using ATMS.Project.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Project.Services.Tests.Fixtures;

namespace Project.Services.Tests.Repositories;

// Invitations and added participants share one limit. The validators alone let parallel requests all
// see a free place; the lock on the project row must let exactly as many through as there are places.
public sealed class ParticipantLimitRaceTest(ProjectPostgreSqlFixture postgres) : IClassFixture<ProjectPostgreSqlFixture>
{
    [Fact]
    public async Task ParallelInvitations_NeverPassTheLimit()
    {
        var project = await postgres.AddProjectAsync();

        var results = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => InviteAsync(project.Id, NewEmail(), 3)));

        Assert.Equal(3, results.Count(result => result is null));
        Assert.All(results.Where(result => result is not null), result =>
            Assert.Equal(WorkProjectParticipantRefusal.LimitReached, result));
        Assert.Equal(3, await CountPendingAsync(project.Id));
    }

    [Fact]
    public async Task ParallelInvitationsOfOneEmail_KeepOnePending()
    {
        var project = await postgres.AddProjectAsync();
        var email = NewEmail();

        var results = await Task.WhenAll(Enumerable.Range(0, 5).Select(_ => InviteAsync(project.Id, email, 20)));

        Assert.Single(results, result => result is null);
        Assert.All(results.Where(result => result is not null), result =>
            Assert.Equal(WorkProjectParticipantRefusal.AlreadyInvited, result));
        Assert.Equal(1, await CountPendingAsync(project.Id));
    }

    [Fact]
    public async Task ParallelAddsAndInvitations_ShareTheSameLimit()
    {
        var project = await postgres.AddProjectAsync();
        var users = new List<User>();
        for (var i = 0; i < 4; i++)
        {
            users.Add(await postgres.AddUserAsync());
        }

        var adds = users.Select(user => AddParticipantAsync(project.Id, user.Id, 4));
        var invitations = Enumerable.Range(0, 4).Select(_ => InviteAsync(project.Id, NewEmail(), 4));
        var results = await Task.WhenAll(adds.Concat(invitations));

        Assert.Equal(4, results.Count(result => result is null));
        await using var context = postgres.CreateContext();
        var participants = await context.WorkProjectParticipants.CountAsync(x => x.WorkProjectId == project.Id);
        Assert.Equal(4, participants + await CountPendingAsync(project.Id));
    }

    private async Task<WorkProjectParticipantRefusal?> InviteAsync(Guid projectId, string email, int limit)
    {
        await using var context = postgres.CreateContext();
        return await new WorkProjectInvitationRepository(context).AddWithinLimitAsync(
            new WorkProjectInvitation
            {
                Id = Guid.NewGuid(),
                WorkProjectId = projectId,
                Email = email,
                NormalizedEmail = email.ToUpperInvariant(),
                Name = "Nigar",
                Surname = "Huseynova",
                RoleId = RoleIds.OrgClientViewer,
                Status = (int)WorkProjectInvitationStatusEnum.Pending,
                InvitedById = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            },
            limit,
            CancellationToken.None);
    }

    private async Task<WorkProjectParticipantRefusal?> AddParticipantAsync(Guid projectId, Guid userId, int limit)
    {
        await using var context = postgres.CreateContext();
        context.WorkProjectParticipants.Add(new WorkProjectParticipant
        {
            UserId = userId,
            WorkProjectId = projectId,
            WorkProjectParticipantRoles = [new WorkProjectParticipantRole { RoleId = RoleIds.OrgClientViewer }]
        });

        return await new WorkProjectRepository(context).SaveParticipantWithinLimitAsync(
            projectId,
            userId,
            limit,
            CancellationToken.None);
    }

    private async Task<int> CountPendingAsync(Guid projectId)
    {
        await using var context = postgres.CreateContext();
        return await context.WorkProjectInvitations.CountAsync(x =>
            x.WorkProjectId == projectId &&
            x.Status == (int)WorkProjectInvitationStatusEnum.Pending);
    }

    private static string NewEmail() => $"{Guid.NewGuid():N}@client.az";
}

using ATMS.Data.Enums;
using ATMS.Data.Interfaces;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Interceptors;
using ATMS.Project.Data.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;

namespace Project.Services.Tests.History;

// The save is stopped right after the history interceptor has run, so what it added is still in the
// change tracker to look at, and no database is needed.
public sealed class ProjectHistoryInterceptorTest
{
    private static readonly Guid ActorId = Guid.NewGuid();
    private static readonly Guid ProjectId = Guid.NewGuid();

    [Fact]
    public async Task SaveChanges_NewTask_RecordsCreatedWithTheFilledFieldsOnly()
    {
        await using var context = CreateContext();
        var task = NewTask();
        context.WorkTasks.Add(task);

        await SaveAsync(context);

        var entry = Assert.Single(Entries(context));
        Assert.Equal((int)HistoryActionEnum.Created, entry.Action);
        Assert.Equal((int)HistoryEntityTypeEnum.WorkTask, entry.EntityType);
        Assert.Equal(task.Id, entry.EntityId);
        Assert.Equal(ProjectId, entry.WorkProjectId);
        Assert.Equal(ActorId, entry.CreatedById);
        Assert.Equal(
            [HistoryFieldEnum.Title, HistoryFieldEnum.Status, HistoryFieldEnum.Priority, HistoryFieldEnum.WorkTicket],
            entry.Changes.Select(change => (HistoryFieldEnum)change.Field).Order());
        Assert.All(entry.Changes, change => Assert.Null(change.OldValue));
        Assert.Equal("Payment gateway", entry.Changes.Single(change => change.Field == (int)HistoryFieldEnum.Title).NewValue);
    }

    [Fact]
    public async Task SaveChanges_StatusAndRankChanged_RecordsOnlyTheStatusWithOldAndNewValue()
    {
        await using var context = CreateContext();
        var task = NewTask();
        context.WorkTasks.Attach(task);

        task.StatusId = (int)WorkTaskStatusEnum.Done;
        task.Rank = "n000000002000v";
        task.DoneAt = DateTime.UtcNow;
        await SaveAsync(context);

        var entry = Assert.Single(Entries(context));
        Assert.Equal((int)HistoryActionEnum.Updated, entry.Action);
        var change = Assert.Single(entry.Changes);
        Assert.Equal((int)HistoryFieldEnum.Status, change.Field);
        Assert.Equal(((int)WorkTaskStatusEnum.New).ToString(), change.OldValue);
        Assert.Equal(((int)WorkTaskStatusEnum.Done).ToString(), change.NewValue);
    }

    [Fact]
    public async Task SaveChanges_OnlyRankChanged_RecordsNothing()
    {
        await using var context = CreateContext();
        var task = NewTask();
        context.WorkTasks.Attach(task);

        task.Rank = "n000000002000v";
        await SaveAsync(context);

        Assert.Empty(Entries(context));
    }

    [Fact]
    public async Task SaveChanges_SameDeadlineReadBackWithAnotherKind_IsNotAChange()
    {
        await using var context = CreateContext();
        var task = NewTask();
        task.Deadline = new DateTime(2026, 9, 30, 0, 0, 0, DateTimeKind.Utc);
        context.WorkTasks.Attach(task);

        task.Deadline = DateTime.SpecifyKind(task.Deadline.Value, DateTimeKind.Unspecified);
        await SaveAsync(context);

        Assert.Empty(Entries(context));
    }

    [Fact]
    public async Task SaveChanges_SoftDeleted_RecordsDeletedWithoutFields()
    {
        await using var context = CreateContext();
        var task = NewTask();
        context.WorkTasks.Attach(task);

        task.IsDeleted = true;
        task.DeletedAt = DateTime.UtcNow;
        task.Title = "Renamed on the way out";
        await SaveAsync(context);

        var entry = Assert.Single(Entries(context));
        Assert.Equal((int)HistoryActionEnum.Deleted, entry.Action);
        Assert.Empty(entry.Changes);
    }

    [Fact]
    public async Task SaveChanges_SubtasksClosedWithTheirTask_RecordsAnEntryForEach()
    {
        await using var context = CreateContext();
        var parent = NewTask();
        var first = NewTask(parent.Id);
        var second = NewTask(parent.Id);
        context.WorkTasks.AttachRange(parent, first, second);

        foreach (var task in new[] { parent, first, second })
        {
            task.StatusId = (int)WorkTaskStatusEnum.Done;
        }

        await SaveAsync(context);

        Assert.Equal(
            new[] { parent.Id, first.Id, second.Id }.Order(),
            Entries(context).Select(entry => entry.EntityId).Order());
    }

    /* The board saves again after someone took the same place a moment earlier. The entries of the
       failed attempt were still tracked as Added and would have been written twice. */
    [Fact]
    public async Task SaveChanges_TriedAgainAfterAFailure_DoesNotRecordTwice()
    {
        await using var context = CreateContext();
        var task = NewTask();
        context.WorkTasks.Attach(task);

        task.StatusId = (int)WorkTaskStatusEnum.InProgress;
        await SaveAsync(context);
        await SaveAsync(context);

        var entry = Assert.Single(Entries(context));
        Assert.Single(entry.Changes);
        Assert.Single(context.ChangeTracker.Entries<HistoryChange>());
    }

    [Fact]
    public void SaveChanges_Synchronous_RecordsTheSameWay()
    {
        using var context = CreateContext();
        var task = NewTask();
        context.WorkTasks.Attach(task);

        task.Title = "Renamed";
        Assert.Throws<SaveStoppedException>(() => context.SaveChanges());

        var change = Assert.Single(Assert.Single(Entries(context)).Changes);
        Assert.Equal((int)HistoryFieldEnum.Title, change.Field);
        Assert.Equal("Payment gateway", change.OldValue);
        Assert.Equal("Renamed", change.NewValue);
    }

    [Fact]
    public async Task SaveChanges_TicketStatus_IsTheTicketStatusNotTheInheritedOne()
    {
        await using var context = CreateContext();
        var ticket = new WorkTicket
        {
            Id = Guid.NewGuid(),
            Code = "28",
            Title = "Payments",
            StatusId = (int)WorkTaskStatusEnum.New,
            WorkTicketStatusId = (int)WorkTicketStatusEnum.New,
            WorkTicketTypeId = 1,
            PriorityId = 2,
            WorkGroupId = Guid.NewGuid(),
            WorkProjectId = ProjectId
        };
        context.WorkTickets.Attach(ticket);

        ticket.WorkTicketStatusId = (int)WorkTicketStatusEnum.Closed;
        ticket.StatusId = (int)WorkTaskStatusEnum.Done;
        await SaveAsync(context);

        var entry = Assert.Single(Entries(context));
        Assert.Equal((int)HistoryEntityTypeEnum.WorkTicket, entry.EntityType);
        var change = Assert.Single(entry.Changes);
        Assert.Equal((int)HistoryFieldEnum.Status, change.Field);
        Assert.Equal(((int)WorkTicketStatusEnum.Closed).ToString(), change.NewValue);
    }

    [Theory]
    [InlineData(null, HistoryEntityTypeEnum.WorkGroup)]
    [InlineData("parent", HistoryEntityTypeEnum.Milestone)]
    public async Task SaveChanges_GroupOrMilestone_BelongsToTheProjectHistory(string? parent, HistoryEntityTypeEnum expected)
    {
        await using var context = CreateContext();
        var group = new WorkGroup
        {
            Id = Guid.NewGuid(),
            Title = "Sprint 1",
            StatusId = 1,
            ParentWorkGroupId = parent is null ? null : Guid.NewGuid(),
            WorkProjectId = ProjectId
        };
        context.WorkGroups.Add(group);

        await SaveAsync(context);

        var entry = Assert.Single(Entries(context));
        Assert.Equal((int)expected, entry.EntityType);
        Assert.Equal(ProjectId, entry.WorkProjectId);
    }

    [Fact]
    public async Task SaveChanges_StakeholderRoleChanged_RecordsOneChangeWithBothRoles()
    {
        await using var context = CreateContext();
        var oldRoleId = Guid.NewGuid();
        var newRoleId = Guid.NewGuid();
        var (project, participant) = ProjectWithParticipant(oldRoleId);
        context.WorkProjects.Attach(project);

        var currentRole = participant.WorkProjectParticipantRoles.Single();
        currentRole.IsDeleted = true;
        participant.WorkProjectParticipantRoles.Add(new WorkProjectParticipantRole { RoleId = newRoleId });
        await SaveAsync(context);

        var entry = Assert.Single(Entries(context));
        Assert.Equal((int)HistoryEntityTypeEnum.Project, entry.EntityType);
        var change = Assert.Single(entry.Changes);
        Assert.Equal((int)HistoryFieldEnum.Stakeholder, change.Field);
        Assert.Equal(participant.UserId, change.SubjectId);
        Assert.Equal(oldRoleId.ToString(), change.OldValue);
        Assert.Equal(newRoleId.ToString(), change.NewValue);
    }

    [Fact]
    public async Task SaveChanges_StakeholderAdded_RecordsTheRoleWithoutAnOldOne()
    {
        await using var context = CreateContext();
        var roleId = Guid.NewGuid();
        var (project, _) = ProjectWithParticipant(Guid.NewGuid());
        context.WorkProjects.Attach(project);

        var userId = Guid.NewGuid();
        project.WorkProjectParticipants.Add(new WorkProjectParticipant
        {
            UserId = userId,
            WorkProjectParticipantRoles = [new WorkProjectParticipantRole { RoleId = roleId }]
        });
        await SaveAsync(context);

        var change = Assert.Single(Assert.Single(Entries(context)).Changes);
        Assert.Equal(userId, change.SubjectId);
        Assert.Null(change.OldValue);
        Assert.Equal(roleId.ToString(), change.NewValue);
    }

    [Fact]
    public async Task SaveChanges_StakeholderRemoved_RecordsTheRoleTheyHad()
    {
        await using var context = CreateContext();
        var roleId = Guid.NewGuid();
        var (project, participant) = ProjectWithParticipant(roleId);
        context.WorkProjects.Attach(project);

        participant.IsDeleted = true;
        participant.WorkProjectParticipantRoles.Single().IsDeleted = true;
        await SaveAsync(context);

        var change = Assert.Single(Assert.Single(Entries(context)).Changes);
        Assert.Equal(roleId.ToString(), change.OldValue);
        Assert.Null(change.NewValue);
    }

    [Fact]
    public async Task SaveChanges_FileAddedAndRenamed_IsRecordedOnItsTask()
    {
        await using var context = CreateContext();
        var task = NewTask();
        var renamed = new Attachment
        {
            Id = Guid.NewGuid(),
            OwnerType = AttachmentOwnerTypeEnum.Task,
            OwnerId = task.Id,
            FileName = "spec.pdf",
            RelativePath = "p/2026/09/a.pdf",
            ContentType = "application/pdf"
        };
        context.WorkTasks.Attach(task);
        context.Attachments.Attach(renamed);

        renamed.FileName = "spec-v2.pdf";
        context.Attachments.Add(new Attachment
        {
            Id = Guid.NewGuid(),
            OwnerType = AttachmentOwnerTypeEnum.Task,
            OwnerId = task.Id,
            FileName = "rates.xlsx",
            RelativePath = "p/2026/09/b.xlsx",
            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        });
        await SaveAsync(context);

        var entry = Assert.Single(Entries(context));
        Assert.Equal(task.Id, entry.EntityId);
        Assert.Equal(ProjectId, entry.WorkProjectId);
        Assert.All(entry.Changes, change => Assert.Equal((int)HistoryFieldEnum.Attachment, change.Field));
        Assert.Contains(entry.Changes, change => change is { OldValue: null, NewValue: "rates.xlsx" });
        Assert.Contains(entry.Changes, change => change is { OldValue: "spec.pdf", NewValue: "spec-v2.pdf" });
    }

    [Fact]
    public async Task SaveChanges_WithoutAnActor_RecordsWithoutAnAuthor()
    {
        await using var context = CreateContext(withActor: false);
        var task = NewTask();
        context.WorkTasks.Attach(task);

        task.Title = "Renamed by a background job";
        await SaveAsync(context);

        Assert.Null(Assert.Single(Entries(context)).CreatedById);
    }

    private static ProjectDbContext CreateContext(bool withActor = true)
    {
        var actor = new Mock<IAuditActorAccessor>();
        actor.Setup(accessor => accessor.UserId).Returns(withActor ? ActorId : null);

        var options = new DbContextOptionsBuilder<ProjectDbContext>()
            .UseNpgsql("Host=localhost;Database=history_tests_only")
            .AddInterceptors(
                new ProjectHistoryInterceptor(new HistoryFieldMap(), actor.Object),
                new StopSavingInterceptor())
            .Options;

        return new ProjectDbContext(options, actor.Object);
    }

    private static async Task SaveAsync(ProjectDbContext context)
    {
        await Assert.ThrowsAsync<SaveStoppedException>(() => context.SaveChangesAsync());
    }

    private static HistoryEntry[] Entries(ProjectDbContext context) =>
        context.ChangeTracker.Entries<HistoryEntry>()
            .Where(entry => entry.State == EntityState.Added)
            .Select(entry => entry.Entity)
            .ToArray();

    private static WorkTask NewTask(Guid? parentId = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            Code = "34",
            Title = "Payment gateway",
            StatusId = (int)WorkTaskStatusEnum.New,
            PriorityId = 2,
            Rank = "n000000001000v",
            ParentWorkTaskId = parentId,
            WorkTicketId = Guid.NewGuid(),
            WorkProjectId = ProjectId
        };

    private static (WorkProject Project, WorkProjectParticipant Participant) ProjectWithParticipant(Guid roleId)
    {
        var participant = new WorkProjectParticipant
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            WorkProjectId = ProjectId,
            WorkProjectParticipantRoles = [new WorkProjectParticipantRole { Id = Guid.NewGuid(), RoleId = roleId }]
        };
        var project = new WorkProject
        {
            Id = ProjectId,
            Code = "7",
            Title = "Checkout",
            ProjectTypeId = 1,
            ProjectKindId = 1,
            ProjectStatusId = 1,
            WorkProjectParticipants = [participant]
        };

        return (project, participant);
    }

    private sealed class SaveStoppedException : Exception;

    private sealed class StopSavingInterceptor : SaveChangesInterceptor
    {
        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result) =>
            throw new SaveStoppedException();

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default) =>
            throw new SaveStoppedException();
    }
}

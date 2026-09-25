using System.Globalization;
using ATMS.Data.Enums;
using ATMS.Data.Interfaces;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ATMS.Project.Data.Interceptors;

/// <summary>
/// Writes the history in the same SaveChanges as the change itself: the change tracker already
/// holds the old and the new value of every property, and one transaction means there is never a
/// change without its history or a history without its change. Handlers write nothing.
/// </summary>
public sealed class ProjectHistoryInterceptor(
    IHistoryFieldMap fieldMap,
    IAuditActorAccessor? auditActor = null) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is ProjectDbContext context)
        {
            var drafts = Collect(context);
            var unknown = TasksWithoutProject(drafts);
            var projects = unknown.Length == 0
                ? new Dictionary<Guid, Guid>()
                : TaskProjects(context, unknown).ToDictionary(task => task.Id, task => task.WorkProjectId);

            Record(context, drafts, projects);
        }

        return result;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is ProjectDbContext context)
        {
            var drafts = Collect(context);
            var unknown = TasksWithoutProject(drafts);
            var projects = unknown.Length == 0
                ? new Dictionary<Guid, Guid>()
                : await TaskProjects(context, unknown)
                    .ToDictionaryAsync(task => task.Id, task => task.WorkProjectId, cancellationToken);

            Record(context, drafts, projects);
        }

        return result;
    }

    private Dictionary<(HistoryEntityTypeEnum, Guid), Draft> Collect(ProjectDbContext context)
    {
        // SaveChanges looks for changes only after the interceptors have run.
        context.ChangeTracker.DetectChanges();
        DiscardUnsaved(context);

        var drafts = new Dictionary<(HistoryEntityTypeEnum, Guid), Draft>();

        foreach (var entry in context.ChangeTracker.Entries().ToArray())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified))
            {
                continue;
            }

            switch (entry.Entity)
            {
                case WorkProject project:
                    CollectItem(drafts, entry, HistoryEntityTypeEnum.Project, project.Id, project.Id);
                    break;
                case WorkGroup group:
                    var groupType = group.ParentWorkGroupId is null
                        ? HistoryEntityTypeEnum.WorkGroup
                        : HistoryEntityTypeEnum.Milestone;
                    CollectItem(drafts, entry, groupType, group.Id, group.WorkProjectId);
                    break;
                case WorkTicket ticket:
                    CollectItem(drafts, entry, HistoryEntityTypeEnum.WorkTicket, ticket.Id, ticket.WorkProjectId);
                    break;
                case WorkTask task:
                    CollectItem(drafts, entry, HistoryEntityTypeEnum.WorkTask, task.Id, task.WorkProjectId);
                    break;
                case Attachment { OwnerType: AttachmentOwnerTypeEnum.Task } attachment:
                    CollectAttachment(context, drafts, entry, attachment);
                    break;
            }
        }

        CollectStakeholders(context, drafts);

        return drafts;
    }

    private void CollectItem(
        Dictionary<(HistoryEntityTypeEnum, Guid), Draft> drafts,
        EntityEntry entry,
        HistoryEntityTypeEnum entityType,
        Guid entityId,
        Guid projectId)
    {
        var draft = GetDraft(drafts, entityType, entityId);
        draft.WorkProjectId = projectId;

        if (entry.State == EntityState.Added)
        {
            draft.Action = HistoryActionEnum.Created;
        }
        else if (IsSoftDeleted(entry))
        {
            draft.Action = HistoryActionEnum.Deleted;
            return;
        }

        var entityClrType = entry.Metadata.ClrType;

        foreach (var property in entry.Properties)
        {
            if (!fieldMap.TryGetField(entityClrType, property.Metadata.Name, out var field))
            {
                continue;
            }

            var newValue = Format(property.CurrentValue);

            if (entry.State == EntityState.Added)
            {
                if (newValue is not null)
                {
                    draft.Changes.Add(new HistoryChange { Field = (int)field, NewValue = newValue });
                }

                continue;
            }

            // DateTime.Equals ignores the kind: the same deadline read back as UTC is not a change.
            if (!property.IsModified || Equals(property.OriginalValue, property.CurrentValue))
            {
                continue;
            }

            var oldValue = Format(property.OriginalValue);
            if (oldValue != newValue)
            {
                draft.Changes.Add(new HistoryChange { Field = (int)field, OldValue = oldValue, NewValue = newValue });
            }
        }
    }

    private static void CollectAttachment(
        ProjectDbContext context,
        Dictionary<(HistoryEntityTypeEnum, Guid), Draft> drafts,
        EntityEntry entry,
        Attachment attachment)
    {
        var fileName = entry.Property(nameof(Attachment.FileName));
        HistoryChange change;

        if (entry.State == EntityState.Added)
        {
            change = new HistoryChange { NewValue = attachment.FileName };
        }
        else if (IsSoftDeleted(entry))
        {
            change = new HistoryChange { OldValue = (string?)fileName.OriginalValue };
        }
        else if (fileName.IsModified && !Equals(fileName.OriginalValue, fileName.CurrentValue))
        {
            change = new HistoryChange
            {
                OldValue = (string?)fileName.OriginalValue,
                NewValue = attachment.FileName
            };
        }
        else
        {
            return;
        }

        change.Field = (int)HistoryFieldEnum.Attachment;
        change.SubjectId = attachment.Id;

        var draft = GetDraft(drafts, HistoryEntityTypeEnum.WorkTask, attachment.OwnerId);
        draft.WorkProjectId ??= context.WorkTasks.Local
            .FirstOrDefault(task => task.Id == attachment.OwnerId)
            ?.WorkProjectId;
        draft.Changes.Add(change);
    }

    // A participant and their role are separate rows, and a new role is a deleted row plus an added
    // one. Here they become one change of the project: who, the role before and the role after.
    private static void CollectStakeholders(
        ProjectDbContext context,
        Dictionary<(HistoryEntityTypeEnum, Guid), Draft> drafts)
    {
        var stakeholders = new Dictionary<WorkProjectParticipant, Stakeholder>(ReferenceEqualityComparer.Instance);

        foreach (var entry in context.ChangeTracker.Entries<WorkProjectParticipant>())
        {
            if (entry.State == EntityState.Added)
            {
                GetStakeholder(stakeholders, entry.Entity).Added = true;
            }
            else if (entry.State == EntityState.Modified && IsSoftDeleted(entry))
            {
                GetStakeholder(stakeholders, entry.Entity).Removed = true;
            }
        }

        foreach (var entry in context.ChangeTracker.Entries<WorkProjectParticipantRole>())
        {
            if (entry.Entity.WorkProjectParticipant is not { } participant)
            {
                continue;
            }

            if (entry.State == EntityState.Added)
            {
                GetStakeholder(stakeholders, participant).NewRoleId = entry.Entity.RoleId;
            }
            else if (entry.State == EntityState.Modified && IsSoftDeleted(entry))
            {
                GetStakeholder(stakeholders, participant).OldRoleId =
                    entry.Property(nameof(WorkProjectParticipantRole.RoleId)).OriginalValue as Guid?;
            }
        }

        foreach (var (participant, stakeholder) in stakeholders)
        {
            var oldRoleId = stakeholder.Added ? null : stakeholder.OldRoleId;
            var newRoleId = stakeholder.Removed ? null : stakeholder.NewRoleId;

            if (stakeholder.Removed && oldRoleId is null)
            {
                oldRoleId = participant.WorkProjectParticipantRoles.FirstOrDefault()?.RoleId;
            }

            if (oldRoleId == newRoleId || (!stakeholder.Added && !stakeholder.Removed && newRoleId is null))
            {
                continue;
            }

            var draft = GetDraft(drafts, HistoryEntityTypeEnum.Project, participant.WorkProjectId);
            draft.WorkProjectId = participant.WorkProjectId;
            draft.Changes.Add(new HistoryChange
            {
                Field = (int)HistoryFieldEnum.Stakeholder,
                SubjectId = participant.UserId,
                OldValue = Format(oldRoleId),
                NewValue = Format(newRoleId)
            });
        }
    }

    private void Record(
        ProjectDbContext context,
        Dictionary<(HistoryEntityTypeEnum, Guid), Draft> drafts,
        IReadOnlyDictionary<Guid, Guid> taskProjects)
    {
        var now = DateTime.UtcNow;
        var actorId = auditActor?.UserId;

        foreach (var draft in drafts.Values)
        {
            var projectId = draft.WorkProjectId
                            ?? (taskProjects.TryGetValue(draft.EntityId, out var taskProjectId) ? taskProjectId : null);

            if (projectId is null || (draft.Action == HistoryActionEnum.Updated && draft.Changes.Count == 0))
            {
                continue;
            }

            context.HistoryEntries.Add(new HistoryEntry
            {
                WorkProjectId = projectId.Value,
                EntityType = (int)draft.EntityType,
                EntityId = draft.EntityId,
                Action = (int)draft.Action,
                CreatedAt = now,
                CreatedById = actorId,
                Changes = draft.Action == HistoryActionEnum.Deleted ? [] : draft.Changes
            });
        }
    }

    // A save that failed — a rank taken a moment earlier on the board — is tried again with the
    // same tracked changes. The entries of the failed attempt would be written twice.
    private static void DiscardUnsaved(ProjectDbContext context)
    {
        var unsaved = context.ChangeTracker.Entries()
            .Where(entry => entry.State == EntityState.Added && entry.Entity is HistoryEntry or HistoryChange)
            .ToArray();

        foreach (var entry in unsaved)
        {
            entry.State = EntityState.Detached;
        }
    }

    private static Guid[] TasksWithoutProject(Dictionary<(HistoryEntityTypeEnum, Guid), Draft> drafts) =>
        drafts.Values
            .Where(draft => draft.WorkProjectId is null)
            .Select(draft => draft.EntityId)
            .ToArray();

    // A file is saved without its task being loaded, so the task's project is read here.
    private static IQueryable<TaskProject> TaskProjects(ProjectDbContext context, Guid[] taskIds) =>
        context.WorkTasks
            .IgnoreQueryFilters()
            .Where(task => taskIds.Contains(task.Id))
            .Select(task => new TaskProject(task.Id, task.WorkProjectId));

    private static bool IsSoftDeleted(EntityEntry entry)
    {
        if (entry.Entity is not ISoftDeletable)
        {
            return false;
        }

        var isDeleted = entry.Property(nameof(ISoftDeletable.IsDeleted));

        return isDeleted.OriginalValue is false && isDeleted.CurrentValue is true;
    }

    private static string? Format(object? value) => value switch
    {
        null => null,
        string text => string.IsNullOrEmpty(text) ? null : text,
        DateTime date => date.ToString("O", CultureInfo.InvariantCulture),
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString()
    };

    private static Draft GetDraft(
        Dictionary<(HistoryEntityTypeEnum, Guid), Draft> drafts,
        HistoryEntityTypeEnum entityType,
        Guid entityId)
    {
        if (!drafts.TryGetValue((entityType, entityId), out var draft))
        {
            draft = new Draft(entityType, entityId);
            drafts.Add((entityType, entityId), draft);
        }

        return draft;
    }

    private static Stakeholder GetStakeholder(
        Dictionary<WorkProjectParticipant, Stakeholder> stakeholders,
        WorkProjectParticipant participant)
    {
        if (!stakeholders.TryGetValue(participant, out var stakeholder))
        {
            stakeholder = new Stakeholder();
            stakeholders.Add(participant, stakeholder);
        }

        return stakeholder;
    }

    private sealed class Draft(HistoryEntityTypeEnum entityType, Guid entityId)
    {
        public HistoryEntityTypeEnum EntityType { get; } = entityType;

        public Guid EntityId { get; } = entityId;

        public Guid? WorkProjectId { get; set; }

        public HistoryActionEnum Action { get; set; } = HistoryActionEnum.Updated;

        public List<HistoryChange> Changes { get; } = [];
    }

    private sealed record TaskProject(Guid Id, Guid WorkProjectId);

    private sealed class Stakeholder
    {
        public bool Added { get; set; }

        public bool Removed { get; set; }

        public Guid? OldRoleId { get; set; }

        public Guid? NewRoleId { get; set; }
    }
}

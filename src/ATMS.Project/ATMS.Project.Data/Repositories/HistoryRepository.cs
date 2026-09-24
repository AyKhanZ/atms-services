using ATMS.Data.Criteria;
using ATMS.Data.Enums;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Models.History;
using ATMS.Project.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ATMS.Project.Data.Repositories;

// The history outlives what it points at: a deleted milestone was still the milestone the ticket
// sat in, and a person who left is still the one who made the change. Every lookup reads past the
// soft-delete filters; the ids come from this project's own history rows.
public class HistoryRepository(ProjectDbContext context) : IHistoryRepository
{
    public async Task<KeysetPagedResult<HistoryEntry>> GetManyAsync(
        ACriteria<HistoryEntry> criteria,
        KeysetPaginationCriteria<HistoryEntry> pagination,
        CancellationToken cancellationToken)
    {
        var query = criteria.Apply(context.HistoryEntries.AsNoTracking());

        var items = await pagination
            .Apply(query, entry => entry.CreatedAt, entry => entry.Id)
            .Include(entry => entry.Changes)
            .ToArrayAsync(cancellationToken);

        return pagination.ToResult(items, entry => entry.CreatedAt, entry => entry.Id);
    }

    public async Task<HistoryStatusChange[]> GetStatusChangesAsync(
        HistoryEntityTypeEnum entityType,
        Guid entityId,
        int limit,
        CancellationToken cancellationToken)
    {
        var type = (int)entityType;
        var latest = await context.HistoryChanges
            .AsNoTracking()
            .Where(change =>
                change.Field == (int)HistoryFieldEnum.Status &&
                change.HistoryEntry.EntityType == type &&
                change.HistoryEntry.EntityId == entityId)
            .OrderByDescending(change => change.HistoryEntry.CreatedAt)
            .ThenByDescending(change => change.HistoryEntry.Id)
            .Take(limit)
            .Select(change => new HistoryStatusChange(
                change.HistoryEntry.CreatedAt,
                change.HistoryEntry.CreatedById,
                change.OldValue,
                change.NewValue))
            .ToArrayAsync(cancellationToken);

        return latest.Reverse().ToArray();
    }

    public Task<HistoryStatusChange?> GetCreationAsync(
        HistoryEntityTypeEnum entityType,
        Guid entityId,
        CancellationToken cancellationToken)
    {
        var type = (int)entityType;
        return context.HistoryEntries
            .AsNoTracking()
            .Where(entry =>
                entry.EntityType == type &&
                entry.EntityId == entityId &&
                entry.Action == (int)HistoryActionEnum.Created)
            .Select(entry => new HistoryStatusChange(entry.CreatedAt, entry.CreatedById, null, null))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<Dictionary<Guid, HistoryPerson>> GetUsersAsync(
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken)
    {
        return context.Users
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(user => userIds.Contains(user.Id))
            .Select(user => new HistoryPerson(user.Id, user.Name, user.Surname, user.AvatarPath))
            .ToDictionaryAsync(person => person.Id, cancellationToken);
    }

    public Task<Dictionary<Guid, HistoryPerson>> GetParticipantsAsync(
        IReadOnlyCollection<Guid> participantIds,
        CancellationToken cancellationToken)
    {
        return context.WorkProjectParticipants
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(participant => participantIds.Contains(participant.Id))
            .Select(participant => new HistoryPerson(
                participant.Id,
                participant.User.Name,
                participant.User.Surname,
                participant.User.AvatarPath))
            .ToDictionaryAsync(person => person.Id, cancellationToken);
    }

    public Task<Dictionary<Guid, HistoryReference>> GetWorkGroupsAsync(
        IReadOnlyCollection<Guid> workGroupIds,
        CancellationToken cancellationToken)
    {
        return context.WorkGroups
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(group => workGroupIds.Contains(group.Id))
            .Select(group => new HistoryReference(group.Id, null, group.Title))
            .ToDictionaryAsync(reference => reference.Id, cancellationToken);
    }

    public Task<Dictionary<Guid, HistoryReference>> GetWorkTicketsAsync(
        IReadOnlyCollection<Guid> workTicketIds,
        CancellationToken cancellationToken)
    {
        return context.WorkTickets
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(ticket => workTicketIds.Contains(ticket.Id))
            .Select(ticket => new HistoryReference(ticket.Id, ticket.Code, ticket.Title))
            .ToDictionaryAsync(reference => reference.Id, cancellationToken);
    }

    public Task<Dictionary<Guid, HistoryReference>> GetWorkTasksAsync(
        IReadOnlyCollection<Guid> workTaskIds,
        CancellationToken cancellationToken)
    {
        return context.WorkTasks
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(task => workTaskIds.Contains(task.Id))
            .Select(task => new HistoryReference(task.Id, task.Code, task.Title))
            .ToDictionaryAsync(reference => reference.Id, cancellationToken);
    }

    public Task<Dictionary<Guid, HistoryReference>> GetOrganizationsAsync(
        IReadOnlyCollection<Guid> organizationIds,
        CancellationToken cancellationToken)
    {
        return context.Organizations
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(organization => organizationIds.Contains(organization.Id))
            .Select(organization => new HistoryReference(organization.Id, null, organization.Title))
            .ToDictionaryAsync(reference => reference.Id, cancellationToken);
    }

    public Task<Dictionary<Guid, HistoryReference>> GetRolesAsync(
        IReadOnlyCollection<Guid> roleIds,
        CancellationToken cancellationToken)
    {
        return context.Roles
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(role => roleIds.Contains(role.Id))
            .Select(role => new HistoryReference(role.Id, null, role.Name))
            .ToDictionaryAsync(reference => reference.Id, cancellationToken);
    }
}

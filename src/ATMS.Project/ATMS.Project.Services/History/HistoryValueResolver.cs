using ATMS.Application.Models;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Models.History;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Models.History;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.History.Interfaces;
using AutoMapper;

namespace ATMS.Project.Services.History;

// The history stores raw values — a status id, a participant id, a date — so that a status reads in
// the caller's language and a renamed milestone shows its current title. Here they become what the
// screen shows, one query per kind of value on the page and none for a kind the page does not hold.
public sealed class HistoryValueResolver(
    IHistoryRepository historyRepository,
    IDictionariesRepository dictionariesRepository,
    IMapper mapper) : IHistoryValueResolver
{
    public async Task<IReadOnlyCollection<HistoryEntryModel>> ResolveEntriesAsync(
        IReadOnlyCollection<HistoryEntry> entries,
        CancellationToken cancellationToken)
    {
        var values = entries
            .SelectMany(entry => entry.Changes.SelectMany(change => new[]
            {
                new RawValue((HistoryEntityTypeEnum)entry.EntityType, (HistoryFieldEnum)change.Field, change.OldValue),
                new RawValue((HistoryEntityTypeEnum)entry.EntityType, (HistoryFieldEnum)change.Field, change.NewValue)
            }))
            .ToArray();
        var userIds = entries
            .Select(entry => entry.CreatedById)
            .Concat(entries
                .SelectMany(entry => entry.Changes)
                .Where(change => change.Field == (int)HistoryFieldEnum.Stakeholder)
                .Select(change => change.SubjectId))
            .OfType<Guid>();
        var groupIds = entries
            .Where(entry => IsGroup((HistoryEntityTypeEnum)entry.EntityType))
            .Select(entry => entry.EntityId);

        var lookup = await LoadAsync(values, userIds, groupIds, cancellationToken);

        return entries
            .Select(entry => new HistoryEntryModel
            {
                Id = entry.Id,
                EntityType = entry.EntityType,
                Action = entry.Action,
                CreatedAt = entry.CreatedAt,
                CreatedBy = lookup.Person(entry.CreatedById),
                Subject = IsGroup((HistoryEntityTypeEnum)entry.EntityType)
                    ? lookup.Reference(lookup.WorkGroups, entry.EntityId.ToString())
                    : null,
                Changes = entry.Changes
                    .OrderBy(change => change.Field)
                    .Select(change => new HistoryChangeModel
                    {
                        Field = change.Field,
                        Person = change.Field == (int)HistoryFieldEnum.Stakeholder
                            ? lookup.Person(change.SubjectId)
                            : null,
                        OldValue = lookup.Optional(
                            (HistoryEntityTypeEnum)entry.EntityType,
                            (HistoryFieldEnum)change.Field,
                            change.OldValue),
                        NewValue = lookup.Optional(
                            (HistoryEntityTypeEnum)entry.EntityType,
                            (HistoryFieldEnum)change.Field,
                            change.NewValue)
                    })
                    .ToArray()
            })
            .ToArray();
    }

    public async Task<IReadOnlyCollection<HistoryStateModel>> ResolveStatesAsync(
        HistoryEntityTypeEnum entityType,
        IReadOnlyCollection<HistoryStatusChange> changes,
        HistoryStatusChange? creation,
        CancellationToken cancellationToken)
    {
        var values = changes
            .SelectMany(change => new[] { change.OldValue, change.NewValue })
            .Select(value => new RawValue(entityType, HistoryFieldEnum.Status, value))
            .ToArray();
        var userIds = changes
            .Append(creation)
            .Select(change => change?.CreatedById)
            .OfType<Guid>();

        var lookup = await LoadAsync(values, userIds, [], cancellationToken);
        var states = new List<HistoryStateModel>();

        // The oldest change is not the creation: the item was created before the history was kept.
        // Its first known status is dated by the creation: almost always the status it was created
        // with, since statuses rarely changed between the creation and the start of the history.
        // Without a creation — a list cut to its latest changes — the date is unknown.
        if (changes.FirstOrDefault() is { OldValue: { } initial })
        {
            states.Add(new HistoryStateModel
            {
                Status = lookup.Value(entityType, HistoryFieldEnum.Status, initial),
                ChangedAt = creation?.CreatedAt,
                ChangedBy = lookup.Person(creation?.CreatedById)
            });
        }

        foreach (var change in changes)
        {
            if (lookup.Optional(entityType, HistoryFieldEnum.Status, change.NewValue) is not { } status)
            {
                continue;
            }

            states.Add(new HistoryStateModel
            {
                Status = status,
                ChangedAt = change.CreatedAt,
                ChangedBy = lookup.Person(change.CreatedById)
            });
        }

        return states;
    }

    private async Task<Lookup> LoadAsync(
        IReadOnlyCollection<RawValue> values,
        IEnumerable<Guid> userIds,
        IEnumerable<Guid> groupIds,
        CancellationToken cancellationToken)
    {
        var present = values.Where(value => value.Value is not null).ToArray();

        Guid[] Ids(HistoryFieldEnum field) => present
            .Where(value => value.Field == field)
            .Select(value => Guid.TryParse(value.Value, out var id) ? id : (Guid?)null)
            .OfType<Guid>()
            .Distinct()
            .ToArray();

        bool Has(HistoryFieldEnum field, params HistoryEntityTypeEnum[] entityTypes) =>
            present.Any(value => value.Field == field && entityTypes.Contains(value.EntityType));

        var people = userIds.Distinct().ToArray();
        var workGroupIds = Ids(HistoryFieldEnum.Milestone).Concat(groupIds).Distinct().ToArray();

        return new Lookup
        {
            Users = await LoadAsync(people, historyRepository.GetUsersAsync, cancellationToken),
            Participants = await LoadAsync(Ids(HistoryFieldEnum.Assignee), historyRepository.GetParticipantsAsync, cancellationToken),
            WorkGroups = await LoadAsync(workGroupIds, historyRepository.GetWorkGroupsAsync, cancellationToken),
            WorkTickets = await LoadAsync(Ids(HistoryFieldEnum.WorkTicket), historyRepository.GetWorkTicketsAsync, cancellationToken),
            WorkTasks = await LoadAsync(Ids(HistoryFieldEnum.ParentWorkTask), historyRepository.GetWorkTasksAsync, cancellationToken),
            Organizations = await LoadAsync(Ids(HistoryFieldEnum.Organization), historyRepository.GetOrganizationsAsync, cancellationToken),
            Roles = await LoadAsync(Ids(HistoryFieldEnum.Stakeholder), historyRepository.GetRolesAsync, cancellationToken),
            ProjectStatuses = Has(HistoryFieldEnum.Status, HistoryEntityTypeEnum.Project)
                ? Translated(await dictionariesRepository.GetProjectStatusesAsync(cancellationToken))
                : [],
            WorkGroupStatuses = Has(HistoryFieldEnum.Status, HistoryEntityTypeEnum.WorkGroup, HistoryEntityTypeEnum.Milestone)
                ? Translated(await dictionariesRepository.GetWorkGroupStatusesAsync(cancellationToken))
                : [],
            WorkTicketStatuses = Has(HistoryFieldEnum.Status, HistoryEntityTypeEnum.WorkTicket)
                ? Translated(await dictionariesRepository.GetWorkTicketStatusesAsync(cancellationToken))
                : [],
            WorkTaskStatuses = Has(HistoryFieldEnum.Status, HistoryEntityTypeEnum.WorkTask)
                ? Translated(await dictionariesRepository.GetWorkTaskStatusesAsync(cancellationToken))
                : [],
            Priorities = Has(HistoryFieldEnum.Priority, HistoryEntityTypeEnum.WorkTicket, HistoryEntityTypeEnum.WorkTask)
                ? Translated(await dictionariesRepository.GetWorkItemPrioritiesAsync(cancellationToken))
                : [],
            ProjectTypes = Has(HistoryFieldEnum.Type, HistoryEntityTypeEnum.Project)
                ? Translated(await dictionariesRepository.GetProjectTypesAsync(cancellationToken))
                : [],
            WorkTicketTypes = Has(HistoryFieldEnum.Type, HistoryEntityTypeEnum.WorkTicket)
                ? Translated(await dictionariesRepository.GetWorkTicketTypesAsync(cancellationToken))
                : [],
            ProjectKinds = Has(HistoryFieldEnum.Kind, HistoryEntityTypeEnum.Project)
                ? Translated(await dictionariesRepository.GetProjectKindsAsync(cancellationToken))
                : [],
            Mapper = mapper
        };
    }

    private static async Task<Dictionary<Guid, T>> LoadAsync<T>(
        Guid[] ids,
        Func<IReadOnlyCollection<Guid>, CancellationToken, Task<Dictionary<Guid, T>>> load,
        CancellationToken cancellationToken) =>
        ids.Length == 0 ? [] : await load(ids, cancellationToken);

    private Dictionary<int, DictionaryModel> Translated<T>(IEnumerable<T> items) =>
        items
            .Select(item => mapper.Map<DictionaryModel>(item))
            .ToDictionary(item => item.Id);

    private static bool IsGroup(HistoryEntityTypeEnum entityType) =>
        entityType is HistoryEntityTypeEnum.WorkGroup or HistoryEntityTypeEnum.Milestone;

    private sealed record RawValue(HistoryEntityTypeEnum EntityType, HistoryFieldEnum Field, string? Value);

    private sealed class Lookup
    {
        public required Dictionary<Guid, HistoryPerson> Users { get; init; }

        public required Dictionary<Guid, HistoryPerson> Participants { get; init; }

        public required Dictionary<Guid, HistoryReference> WorkGroups { get; init; }

        public required Dictionary<Guid, HistoryReference> WorkTickets { get; init; }

        public required Dictionary<Guid, HistoryReference> WorkTasks { get; init; }

        public required Dictionary<Guid, HistoryReference> Organizations { get; init; }

        public required Dictionary<Guid, HistoryReference> Roles { get; init; }

        public required Dictionary<int, DictionaryModel> ProjectStatuses { get; init; }

        public required Dictionary<int, DictionaryModel> WorkGroupStatuses { get; init; }

        public required Dictionary<int, DictionaryModel> WorkTicketStatuses { get; init; }

        public required Dictionary<int, DictionaryModel> WorkTaskStatuses { get; init; }

        public required Dictionary<int, DictionaryModel> Priorities { get; init; }

        public required Dictionary<int, DictionaryModel> ProjectTypes { get; init; }

        public required Dictionary<int, DictionaryModel> WorkTicketTypes { get; init; }

        public required Dictionary<int, DictionaryModel> ProjectKinds { get; init; }

        public required IMapper Mapper { get; init; }

        public HistoryPersonModel? Person(Guid? userId) =>
            userId is { } id && Users.TryGetValue(id, out var person)
                ? Mapper.Map<HistoryPersonModel>(person)
                : null;

        public HistoryValueModel? Optional(HistoryEntityTypeEnum entityType, HistoryFieldEnum field, string? raw) =>
            raw is null ? null : Value(entityType, field, raw);

        public HistoryValueModel Value(HistoryEntityTypeEnum entityType, HistoryFieldEnum field, string raw)
        {
            return field switch
            {
                HistoryFieldEnum.Status => Dictionary(StatusesOf(entityType), raw),
                HistoryFieldEnum.Priority => Dictionary(Priorities, raw),
                HistoryFieldEnum.Type => Dictionary(
                    entityType == HistoryEntityTypeEnum.Project ? ProjectTypes : WorkTicketTypes,
                    raw),
                HistoryFieldEnum.Kind => Dictionary(ProjectKinds, raw),
                HistoryFieldEnum.Assignee => Participant(raw),
                HistoryFieldEnum.Milestone => Reference(WorkGroups, raw),
                HistoryFieldEnum.WorkTicket => Reference(WorkTickets, raw),
                HistoryFieldEnum.ParentWorkTask => Reference(WorkTasks, raw),
                HistoryFieldEnum.Organization => Reference(Organizations, raw),
                HistoryFieldEnum.Stakeholder => Reference(Roles, raw),
                // Title, description, dates and file names are shown as they were written.
                _ => new HistoryValueModel { Id = raw, Name = raw }
            };
        }

        public HistoryValueModel Reference(Dictionary<Guid, HistoryReference> references, string raw) =>
            Guid.TryParse(raw, out var id) && references.TryGetValue(id, out var reference)
                ? new HistoryValueModel { Id = raw, Code = reference.Code, Name = reference.Name }
                : new HistoryValueModel { Id = raw };

        private Dictionary<int, DictionaryModel> StatusesOf(HistoryEntityTypeEnum entityType) => entityType switch
        {
            HistoryEntityTypeEnum.Project => ProjectStatuses,
            HistoryEntityTypeEnum.WorkGroup or HistoryEntityTypeEnum.Milestone => WorkGroupStatuses,
            HistoryEntityTypeEnum.WorkTicket => WorkTicketStatuses,
            _ => WorkTaskStatuses
        };

        private static HistoryValueModel Dictionary(Dictionary<int, DictionaryModel> items, string raw) =>
            int.TryParse(raw, out var id) && items.TryGetValue(id, out var item)
                ? new HistoryValueModel { Id = raw, Code = item.Code, Name = item.Name }
                : new HistoryValueModel { Id = raw };

        private HistoryValueModel Participant(string raw) =>
            Guid.TryParse(raw, out var id) && Participants.TryGetValue(id, out var person)
                ? new HistoryValueModel
                {
                    Id = raw,
                    Name = $"{person.Name} {person.Surname}",
                    Person = Mapper.Map<HistoryPersonModel>(person)
                }
                : new HistoryValueModel { Id = raw };
    }
}

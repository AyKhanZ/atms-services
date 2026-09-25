using ATMS.Data.Enums;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Services.Interfaces;

namespace ATMS.Project.Data.Services;

// A white list: a column nobody has decided about stays out of the history. The test over the EF
// model fails for a property that is in neither list, so a new column cannot slip past unnoticed.
public sealed class HistoryFieldMap : IHistoryFieldMap
{
    private static readonly string[] AuditProperties =
    [
        "Id",
        "CreatedById",
        "CreatedAt",
        "UpdatedById",
        "UpdatedAt",
        "IsDeleted",
        "DeletedById",
        "DeletedAt"
    ];

    private readonly Dictionary<Type, Dictionary<string, HistoryFieldEnum>> _fields = new()
    {
        [typeof(WorkProject)] = new()
        {
            [nameof(WorkProject.Title)] = HistoryFieldEnum.Title,
            [nameof(WorkProject.Description)] = HistoryFieldEnum.Description,
            [nameof(WorkProject.OrganizationId)] = HistoryFieldEnum.Organization,
            [nameof(WorkProject.ProjectTypeId)] = HistoryFieldEnum.Type,
            [nameof(WorkProject.ProjectKindId)] = HistoryFieldEnum.Kind,
            [nameof(WorkProject.ProjectStatusId)] = HistoryFieldEnum.Status,
            [nameof(WorkProject.StartDate)] = HistoryFieldEnum.StartDate,
            [nameof(WorkProject.EndDate)] = HistoryFieldEnum.EndDate
        },
        [typeof(WorkGroup)] = new()
        {
            [nameof(WorkGroup.Title)] = HistoryFieldEnum.Title,
            [nameof(WorkGroup.StatusId)] = HistoryFieldEnum.Status
        },
        [typeof(WorkTicket)] = new()
        {
            [nameof(WorkTicket.Title)] = HistoryFieldEnum.Title,
            [nameof(WorkTicket.Description)] = HistoryFieldEnum.Description,
            [nameof(WorkTicket.WorkTicketTypeId)] = HistoryFieldEnum.Type,
            [nameof(WorkTicket.WorkTicketStatusId)] = HistoryFieldEnum.Status,
            [nameof(WorkTicket.PriorityId)] = HistoryFieldEnum.Priority,
            [nameof(WorkTicket.AssigneeId)] = HistoryFieldEnum.Assignee,
            [nameof(WorkTicket.Deadline)] = HistoryFieldEnum.Deadline,
            [nameof(WorkTicket.WorkGroupId)] = HistoryFieldEnum.Milestone
        },
        [typeof(WorkTask)] = new()
        {
            [nameof(WorkTask.Title)] = HistoryFieldEnum.Title,
            [nameof(WorkTask.Description)] = HistoryFieldEnum.Description,
            [nameof(WorkTask.StatusId)] = HistoryFieldEnum.Status,
            [nameof(WorkTask.PriorityId)] = HistoryFieldEnum.Priority,
            [nameof(WorkTask.AssigneeId)] = HistoryFieldEnum.Assignee,
            [nameof(WorkTask.Deadline)] = HistoryFieldEnum.Deadline,
            [nameof(WorkTask.WorkTicketId)] = HistoryFieldEnum.WorkTicket,
            [nameof(WorkTask.ParentWorkTaskId)] = HistoryFieldEnum.ParentWorkTask
        }
    };

    private readonly Dictionary<Type, HashSet<string>> _ignored = new()
    {
        [typeof(WorkProject)] = [..AuditProperties, nameof(WorkProject.Code)],
        // A milestone never changes its group, and the project is fixed from the start.
        [typeof(WorkGroup)] = [..AuditProperties, nameof(WorkGroup.ParentWorkGroupId), nameof(WorkGroup.WorkProjectId)],
        // The ticket's own status is WorkTicketStatusId; StatusId is inherited from the task and always New.
        [typeof(WorkTicket)] =
        [
            ..AuditProperties,
            nameof(WorkTicket.Code),
            nameof(WorkTicket.StatusId),
            nameof(WorkTicket.WorkProjectId)
        ],
        // Rank changes with every drag inside a column, DoneAt follows the status: both are noise.
        [typeof(WorkTask)] =
        [
            ..AuditProperties,
            nameof(WorkTask.Code),
            nameof(WorkTask.Rank),
            nameof(WorkTask.DoneAt),
            nameof(WorkTask.WorkProjectId)
        ]
    };

    public IReadOnlyCollection<Type> EntityTypes => _fields.Keys;

    public bool TryGetField(Type entityType, string propertyName, out HistoryFieldEnum field)
    {
        field = default;

        return _fields.TryGetValue(entityType, out var fields)
               && fields.TryGetValue(propertyName, out field);
    }

    public bool IsIgnored(Type entityType, string propertyName) =>
        _ignored.TryGetValue(entityType, out var ignored) && ignored.Contains(propertyName);
}

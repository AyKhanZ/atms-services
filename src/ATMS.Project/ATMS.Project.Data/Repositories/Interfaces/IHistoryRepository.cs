using ATMS.Data.Criteria;
using ATMS.Data.Enums;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Models.History;

namespace ATMS.Project.Data.Repositories.Interfaces;

public interface IHistoryRepository
{
    Task<KeysetPagedResult<HistoryEntry>> GetManyAsync(
        ACriteria<HistoryEntry> criteria,
        KeysetPaginationCriteria<HistoryEntry> pagination,
        CancellationToken cancellationToken);

    Task<HistoryStatusChange[]> GetStatusChangesAsync(
        HistoryEntityTypeEnum entityType,
        Guid entityId,
        int limit,
        CancellationToken cancellationToken);

    Task<HistoryStatusChange?> GetCreationAsync(
        HistoryEntityTypeEnum entityType,
        Guid entityId,
        CancellationToken cancellationToken);

    Task<Dictionary<Guid, HistoryPerson>> GetUsersAsync(
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken);

    Task<Dictionary<Guid, HistoryPerson>> GetParticipantsAsync(
        IReadOnlyCollection<Guid> participantIds,
        CancellationToken cancellationToken);

    Task<Dictionary<Guid, HistoryReference>> GetWorkGroupsAsync(
        IReadOnlyCollection<Guid> workGroupIds,
        CancellationToken cancellationToken);

    Task<Dictionary<Guid, HistoryReference>> GetWorkTicketsAsync(
        IReadOnlyCollection<Guid> workTicketIds,
        CancellationToken cancellationToken);

    Task<Dictionary<Guid, HistoryReference>> GetWorkTasksAsync(
        IReadOnlyCollection<Guid> workTaskIds,
        CancellationToken cancellationToken);

    Task<Dictionary<Guid, HistoryReference>> GetOrganizationsAsync(
        IReadOnlyCollection<Guid> organizationIds,
        CancellationToken cancellationToken);

    Task<Dictionary<Guid, HistoryReference>> GetRolesAsync(
        IReadOnlyCollection<Guid> roleIds,
        CancellationToken cancellationToken);
}

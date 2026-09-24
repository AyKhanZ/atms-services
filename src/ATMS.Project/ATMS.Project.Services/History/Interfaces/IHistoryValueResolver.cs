using ATMS.Data.Enums;
using ATMS.Project.Contracts.Models.History;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Models.History;

namespace ATMS.Project.Services.History.Interfaces;

public interface IHistoryValueResolver
{
    Task<IReadOnlyCollection<HistoryEntryModel>> ResolveEntriesAsync(
        IReadOnlyCollection<HistoryEntry> entries,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<HistoryStateModel>> ResolveStatesAsync(
        HistoryEntityTypeEnum entityType,
        IReadOnlyCollection<HistoryStatusChange> changes,
        HistoryStatusChange? creation,
        CancellationToken cancellationToken);
}

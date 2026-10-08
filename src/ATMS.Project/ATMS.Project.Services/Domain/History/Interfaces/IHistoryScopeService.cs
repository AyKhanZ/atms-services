using ATMS.Project.Services.Models.History;

namespace ATMS.Project.Services.Domain.History.Interfaces;

public interface IHistoryScopeService
{
    Task<HistoryScope> ResolveAsync(
        Guid projectId,
        Guid? workTicketId,
        Guid? workTaskId,
        CancellationToken cancellationToken);
}

using ATMS.Project.Data.Entities.Dictionaries;

namespace ATMS.Project.Data.Repositories.Interfaces;

public interface IDictionariesRepository
{
    Task<bool> IsProjectKindExistAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> IsProjectStatusExistAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> IsProjectTypeExistAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> IsWorkItemPriorityExistAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> IsWorkTicketTypeExistAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> IsWorkTicketStatusExistAsync(int id, CancellationToken cancellationToken = default);

    Task<List<ProjectKind>> GetProjectKindsAsync(CancellationToken cancellationToken = default);
    
    Task<List<ProjectStatus>> GetProjectStatusesAsync(CancellationToken cancellationToken = default);
    
    Task<List<ProjectType>> GetProjectTypesAsync(CancellationToken cancellationToken = default);
    
    
    Task<List<WorkGroupStatus>> GetWorkGroupStatusesAsync(CancellationToken cancellationToken = default);
    
    
    Task<List<WorkItemPriority>> GetWorkItemPrioritiesAsync(CancellationToken cancellationToken = default);
    
    
    Task<List<WorkTaskStatus>> GetWorkTaskStatusesAsync(CancellationToken cancellationToken = default);
    
    
    Task<List<WorkTicketStatus>> GetWorkTicketStatusesAsync(CancellationToken cancellationToken = default);
    
    
    Task<List<WorkTicketType>> GetWorkTicketTypesAsync(CancellationToken cancellationToken = default);
}

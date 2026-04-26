using ATMS.Project.Data.Entities.Dictionaries;

namespace ATMS.Project.Data.Repositories.Interfaces;

public interface IDictionariesRepository
{
    Task<List<ProjectKind>> GetProjectKindsAsync(CancellationToken cancellationToken = default);
    
    Task<List<ProjectStatus>> GetProjectStatusesAsync(CancellationToken cancellationToken = default);
    
    Task<List<ProjectType>> GetProjectTypesAsync(CancellationToken cancellationToken = default);
    
    
    Task<List<WorkGroupStatus>> GetWorkGroupStatusesAsync(CancellationToken cancellationToken = default);
    
    
    Task<List<WorkItemPriority>> GetWorkItemPrioritiesAsync(CancellationToken cancellationToken = default);
    
    
    Task<List<WorkTaskStatus>> GetWorkTaskStatusesAsync(CancellationToken cancellationToken = default);
    
    
    Task<List<WorkTicketStatus>> GetWorkTicketStatusesAsync(CancellationToken cancellationToken = default);
    
    
    Task<List<WorkTicketType>> GetWorkTicketTypesAsync(CancellationToken cancellationToken = default);
}

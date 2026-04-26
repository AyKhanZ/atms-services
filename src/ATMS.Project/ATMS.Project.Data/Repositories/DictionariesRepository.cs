using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Entities.Dictionaries;
using ATMS.Project.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ATMS.Project.Data.Repositories;

public class DictionariesRepository(ProjectDbContext context) : IDictionariesRepository
{
    public Task<List<ProjectKind>> GetProjectKindsAsync(CancellationToken cancellationToken = default)
    {
        return context.ProjectKinds
            .Include(p => p.Translations)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<List<ProjectStatus>> GetProjectStatusesAsync(CancellationToken cancellationToken = default)
    {
        return context.ProjectStatuses
            .Include(p => p.Translations)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<List<ProjectType>> GetProjectTypesAsync(CancellationToken cancellationToken = default)
    {
        return context.ProjectTypes
            .Include(p => p.Translations)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<List<WorkGroupStatus>> GetWorkGroupStatusesAsync(CancellationToken cancellationToken = default)
    {
        return context.WorkGroupStatuses
            .Include(p => p.Translations)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<List<WorkItemPriority>> GetWorkItemPrioritiesAsync(CancellationToken cancellationToken = default)
    {
        return context.WorkItemPriorities
            .Include(p => p.Translations)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<List<WorkTaskStatus>> GetWorkTaskStatusesAsync(CancellationToken cancellationToken = default)
    {
        return context.WorkTaskStatuses
            .Include(p => p.Translations)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<List<WorkTicketStatus>> GetWorkTicketStatusesAsync(CancellationToken cancellationToken = default)
    {
        return context.WorkTicketStatuses
            .Include(p => p.Translations)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<List<WorkTicketType>> GetWorkTicketTypesAsync(CancellationToken cancellationToken = default)
    {
        return context.WorkTicketTypes
            .Include(p => p.Translations)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}

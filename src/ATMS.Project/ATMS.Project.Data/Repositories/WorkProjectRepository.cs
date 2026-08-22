using System.Linq.Expressions;
using ATMS.Data.Criteria;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ATMS.Project.Data.Repositories;

public class WorkProjectRepository(ProjectDbContext context) : IWorkProjectRepository
{
    public Task<WorkProject?> GetAsync(
        Guid id,
        ACriteria<WorkProject> accessCriteria,
        CancellationToken cancellationToken)
    {
        var query = DetailsQuery()
            .AsNoTracking()
            .Where(x => x.Id == id);

        return accessCriteria.Apply(query).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResult<WorkProject>> GetAsync(
        ACriteria<WorkProject> filterCriteria,
        PaginationCriteria<WorkProject> pagination,
        CancellationToken cancellationToken)
    {
        var query = context.WorkProjects
            .Include(x => x.Organization)
            .Include(x => x.ProjectType).ThenInclude(x => x.Translations)
            .Include(x => x.ProjectKind).ThenInclude(x => x.Translations)
            .Include(x => x.ProjectStatus).ThenInclude(x => x.Translations)
            .AsNoTracking();

        query = filterCriteria.Apply(query);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await pagination.Apply(query).ToArrayAsync(cancellationToken);

        return new PagedResult<WorkProject>
        {
            Items = items,
            TotalCount = totalCount,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    public Task<WorkProject?> FindAsync(Guid id, CancellationToken cancellationToken)
    {
        return DetailsQuery().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<WorkProject?> FindRootAsync(Guid id, CancellationToken cancellationToken)
    {
        return context.WorkProjects.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task CreateAsync(WorkProject entity, CancellationToken cancellationToken)
    {
        await context.WorkProjects.AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public void Touch(WorkProject entity)
    {
        context.Entry(entity).State = EntityState.Modified;
    }

    public Task<bool> IsExistAsync(
        Expression<Func<WorkProject, bool>> predicate,
        CancellationToken cancellationToken)
    {
        return context.WorkProjects.AnyAsync(predicate, cancellationToken);
    }

    public Task SaveAsync(CancellationToken cancellationToken)
    {
        return context.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<WorkProject> DetailsQuery()
    {
        return context.WorkProjects
            .Include(x => x.Organization)
            .Include(x => x.ProjectType).ThenInclude(x => x.Translations)
            .Include(x => x.ProjectKind).ThenInclude(x => x.Translations)
            .Include(x => x.ProjectStatus).ThenInclude(x => x.Translations)
            .Include(x => x.UpdatedBy)
            .Include(x => x.WorkProjectParticipants).ThenInclude(x => x.User)
            .Include(x => x.WorkProjectParticipants)
                .ThenInclude(x => x.WorkProjectParticipantRoles)
                .ThenInclude(x => x.Role);
    }
}

using System.Linq.Expressions;
using ATMS.Data.Criterias;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ATMS.Project.Data.Repositories;

public class OrganizationRepository(ProjectDbContext context) : IOrganizationRepository
{
    public async Task<Organization?> GetAsync(Expression<Func<Organization, bool>> predicate, CancellationToken cancellationToken)
    {
        return await context.Organizations
            .Include(r => r.Users)
            .AsNoTracking()
            .FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<PagedResult<Organization>> GetAsync(
        ACriteria<Organization> filterCriteria,
        PaginationCriteria<Organization> pagination,
        CancellationToken cancellationToken)
    {
        var query = context.Organizations
            .AsNoTracking()
            .AsSplitQuery();
        
        query = filterCriteria.Apply(query);
        
        var totalCount = await query.CountAsync(cancellationToken);
        
        var result = await pagination.Apply(query).ToListAsync(cancellationToken);
        
        return new PagedResult<Organization>
        {
            Items      = result.ToArray(),
            TotalCount = totalCount,
            Page       = pagination.Page,
            PageSize   = pagination.PageSize
        };
    }

    public Task<Organization?> FindAsync(Expression<Func<Organization, bool>> predicate, CancellationToken cancellationToken)
    {
        return context.Organizations
            .Include(r => r.Users)
            .FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task CreateAsync(Organization entity, CancellationToken cancellationToken)
    {
        await context.Organizations.AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> IsExistAsync(Expression<Func<Organization, bool>> predicate, CancellationToken cancellationToken)
        => context.Organizations.AnyAsync(predicate, cancellationToken);

    public Task SaveAsync(CancellationToken cancellationToken)
    {
        return context.SaveChangesAsync(cancellationToken);
    }
}

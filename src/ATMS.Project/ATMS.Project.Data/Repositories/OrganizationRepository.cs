using System.Linq.Expressions;
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

    public Task<List<Organization>> GetAsync(CancellationToken cancellationToken)
    {
        return context.Organizations
            .AsNoTracking()
            .ToListAsync(cancellationToken);
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

using System.Linq.Expressions;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ATMS.Project.Data.Repositories;

public class UserRepository(ProjectDbContext context) : IUserRepository
{
    public async Task AddAsync(User entity, CancellationToken cancellationToken)
    {
        await context.Users.AddAsync(entity, cancellationToken);
    }

    public async Task CreateAsync(User entity, CancellationToken cancellationToken)
    {
        await context.Users.AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
    
    public Task<User?> FindAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken)
    {
        return context.Users
            .FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<User?> GetAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken)
    {
        return await context.Users
            .Include(r => r.Organization)
            .AsNoTracking()
            .FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public Task<List<User>> GetAsync(CancellationToken cancellationToken)
    {
        return context.Users
            .Include(r => r.Organization)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<List<User>> GetManyAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        return context.Users
            .AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(cancellationToken);
    }

    public Task<bool> IsExistAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken)
        => context.Users.AnyAsync(predicate, cancellationToken);
    
    public Task SaveAsync(CancellationToken cancellationToken)
    {
        return context.SaveChangesAsync(cancellationToken);
    }
}

using System.Linq.Expressions;
using ATMS.Admin.Data.DbContexts;
using ATMS.Admin.Data.Entities.UserProgresses;
using ATMS.Admin.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ATMS.Admin.Data.Repositories;

public class UserProgressRepository(AdminDbContext context) : IUserProgressRepository
{
    public Task<UserProgress?> FindAsync(Expression<Func<UserProgress, bool>> predicate, CancellationToken cancellationToken)
    {
        return context.UserProgresses
            .Include(x => x.User)
            .ThenInclude(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .Include(x => x.PersonalInfo)
            .Include(x => x.InvitedUsers)
            .AsSplitQuery()
            .FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public Task<UserProgress?> GetAsync(Guid userId, CancellationToken cancellationToken)
    {
        return context.UserProgresses
            .Include(x => x.User)
            .ThenInclude(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .Include(x => x.PersonalInfo)
            .Include(x => x.InvitedUsers)
            .AsSplitQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }

    public async Task CreateAsync(UserProgress userProgress, CancellationToken cancellationToken)
    {
        await context.UserProgresses.AddAsync(userProgress, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> IsExistAsync(Expression<Func<UserProgress, bool>> predicate, CancellationToken cancellationToken)
        => context.UserProgresses.AnyAsync(predicate, cancellationToken);
    
    public Task SaveAsync(CancellationToken cancellationToken) => context.SaveChangesAsync(cancellationToken);
}
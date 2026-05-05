using System.Linq.Expressions;
using ATMS.Admin.Data.DbContexts;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Entities.UserProgresses;
using ATMS.Admin.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ATMS.Admin.Data.Repositories;

public class UserProgressRepository(AdminDbContext context) : IUserProgressRepository
{
    public async Task SubmitAsync(UserProgress progress, User user, CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var pi = progress.PersonalInfo!;

            user.Name = pi.Name;
            user.Surname = pi.Surname;
            user.Email = pi.Email;
            user.PhoneNumber = pi.PhoneNumber;
            user.Position = pi.Position;
            user.Language = pi.Language;
            user.AvatarPath = pi.AvatarPath;
            user.BirthDate = pi.BirthDate;
            user.GenderId = pi.GenderId;
            user.MaritalStatusId = pi.MaritalStatusId;
            user.PasswordHash = progress.PasswordHash!;
            user.OrganizationId = progress.OrganizationId;
            user.HasCompletedSurvey = true;

            context.UserProgresses.Remove(progress);
            await context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public Task<UserProgress?> FindAsync(Expression<Func<UserProgress, bool>> predicate,
        CancellationToken cancellationToken)
    {
        return context.UserProgresses
            .Include(x => x.PersonalInfo)
            .Include(x => x.InvitedUsers)
            .AsSplitQuery()
            .FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public Task<UserProgress?> GetAsync(Guid userId, CancellationToken cancellationToken)
    {
        return context.UserProgresses
            .Include(x => x.PersonalInfo)
            .Include(x => x.InvitedUsers)
            .AsSplitQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }

    public async Task CreateAsync(UserProgress userProgress, CancellationToken cancellationToken)
    {
        await context.UserProgresses.AddAsync(userProgress, cancellationToken);
    }

    public Task<bool> IsExistAsync(Expression<Func<UserProgress, bool>> predicate, CancellationToken cancellationToken)
        => context.UserProgresses.AnyAsync(predicate, cancellationToken);

    public Task SaveAsync(CancellationToken cancellationToken) => context.SaveChangesAsync(cancellationToken);
}
using ATMS.Admin.Data.DbContexts;
using ATMS.Admin.Data.Entities.Onboarding;
using ATMS.Admin.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ATMS.Admin.Data.Repositories;

public class OnboardingRepository(AdminDbContext context) : IOnboardingRepository
{
    public Task<OnboardingProgress?> GetAsync(Guid userId, CancellationToken cancellationToken)
    {
        return Query().FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }

    public Task<OnboardingProgress?> GetAsNoTrackingAsync(Guid userId, CancellationToken cancellationToken)
    {
        return Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }

    public async Task<OnboardingProgress?> GetOrCreateAsync(Guid userId, CancellationToken cancellationToken)
    {
        var progress = await GetAsync(userId, cancellationToken);
        if (progress is not null)
        {
            return progress;
        }

        if (!await context.Users.AnyAsync(x => x.Id == userId, cancellationToken))
        {
            return null;
        }

        await AddAsync(new OnboardingProgress
        {
            UserId = userId,
            UpdatedAt = DateTime.UtcNow
        }, cancellationToken);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            context.ChangeTracker.Clear();
        }

        return await GetAsync(userId, cancellationToken);
    }

    public async Task AddAsync(OnboardingProgress progress, CancellationToken cancellationToken)
    {
        await context.OnboardingProgresses.AddAsync(progress, cancellationToken);
    }

    public Task<bool> IsInvitedEmailInUseAsync(string normalizedEmail, Guid onboardingUserId, CancellationToken cancellationToken)
    {
        return context.OnboardingInvitedUsers.AnyAsync(x => x.NormalizedEmail == normalizedEmail && x.OnboardingUserId != onboardingUserId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<string>> GetEmailsInUseAsync(IReadOnlyCollection<string> normalizedEmails, Guid onboardingUserId, CancellationToken cancellationToken)
    {
        var userEmails = await context.Users
            .Where(x => normalizedEmails.Contains(x.NormalizedEmail))
            .Select(x => x.NormalizedEmail)
            .ToArrayAsync(cancellationToken);
        
        var invitedEmails = await context.OnboardingInvitedUsers
            .Where(x => x.OnboardingUserId != onboardingUserId && normalizedEmails.Contains(x.NormalizedEmail))
            .Select(x => x.NormalizedEmail)
            .ToArrayAsync(cancellationToken);

        return userEmails
            .Concat(invitedEmails)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    public Task<bool> TrySaveAsync(OnboardingProgress progress, long expectedVersion, CancellationToken cancellationToken)
    {
        return SaveCoreAsync(progress, expectedVersion, cancellationToken);
    }

    private async Task<bool> SaveCoreAsync(OnboardingProgress progress, long expectedVersion, CancellationToken cancellationToken)
    {
        if (progress.Version != expectedVersion)
        {
            return false;
        }

        progress.Version++;
        progress.UpdatedAt = DateTime.UtcNow;

        try
        {
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateConcurrencyException exception) when (
            exception.Entries.Count == 1 &&
            exception.Entries[0].Entity is OnboardingProgress)
        {
            return false;
        }
    }

    private IQueryable<OnboardingProgress> Query()
    {
        return context.OnboardingProgresses
            .Include(x => x.User)
            .ThenInclude(x => x.UserRoles)
            .Include(x => x.PersonalInfo)
            .Include(x => x.InvitedUsers)
            .AsSplitQuery();
    }
}

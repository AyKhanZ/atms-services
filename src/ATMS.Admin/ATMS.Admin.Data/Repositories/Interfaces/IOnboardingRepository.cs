using ATMS.Admin.Data.Entities.Onboarding;

namespace ATMS.Admin.Data.Repositories.Interfaces;

public interface IOnboardingRepository
{
    Task<OnboardingProgress?> GetAsync(Guid userId, CancellationToken cancellationToken);

    Task<bool> IsInvitedEmailInUseAsync(string normalizedEmail, Guid onboardingUserId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<string>> GetEmailsInUseAsync(IReadOnlyCollection<string> normalizedEmails, Guid onboardingUserId, CancellationToken cancellationToken);

    Task<bool> TrySaveAsync(OnboardingProgress progress, long expectedVersion, CancellationToken cancellationToken);

}

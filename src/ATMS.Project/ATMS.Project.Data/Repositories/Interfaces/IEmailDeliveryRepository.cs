using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Models.Notifications;

namespace ATMS.Project.Data.Repositories.Interfaces;

public interface IEmailDeliveryRepository
{
    Task AddRangeAsync(IEnumerable<EmailDelivery> deliveries, CancellationToken cancellationToken);

    Task<EmailDelivery[]> ClaimPendingAsync(int batchSize, CancellationToken cancellationToken);

    Task<EmailDeliveryRow?> GetAsync(Guid id, CancellationToken cancellationToken);

    Task MarkProcessedAsync(Guid id, CancellationToken cancellationToken);

    Task MarkRetryAsync(
        Guid id,
        int attemptCount,
        DateTime nextAttemptAt,
        string error,
        CancellationToken cancellationToken);

    Task MarkFailedAsync(
        Guid id,
        int attemptCount,
        string error,
        CancellationToken cancellationToken);
}

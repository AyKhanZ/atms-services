using ATMS.Admin.Data.Entities.Messaging;
using ATMS.Data.Enums;

namespace ATMS.Admin.Data.Repositories.Interfaces;

public interface IEmailDeliveryRepository
{
    Task<Guid> AddConfirmationAsync(
        Guid userId,
        string temporaryPassword,
        CancellationToken cancellationToken);

    Task<Guid> AddPasswordResetAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task RemoveUnsentAsync(
        Guid userId,
        EmailDeliveryTypeEnum type,
        CancellationToken cancellationToken);

    Task<List<EmailDelivery>> ClaimPendingAsync(
        int batchSize,
        CancellationToken cancellationToken);

    Task<EmailDelivery?> GetAsync(Guid id, CancellationToken cancellationToken);

    Task SetPasswordResetTokenAsync(
        Guid id,
        string token,
        DateTime expiresAt,
        CancellationToken cancellationToken);

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

    Task DeleteProcessedBeforeAsync(
        DateTime processedBefore,
        CancellationToken cancellationToken);
}

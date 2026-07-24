using ATMS.Admin.Data.DbContexts;
using ATMS.Admin.Data.Entities.Messaging;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace ATMS.Admin.Data.Repositories;

public class EmailDeliveryRepository(AdminDbContext context) : IEmailDeliveryRepository
{
    public async Task<Guid> AddConfirmationAsync(
        Guid userId,
        string temporaryPassword,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var entity = new EmailDelivery
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = EmailDeliveryTypeEnum.Confirmation,
            TemporaryPassword = temporaryPassword,
            Status = DeliveryStatusEnum.Pending,
            CreatedAt = now,
            NextAttemptAt = now
        };

        await context.EmailDeliveries.AddAsync(entity, cancellationToken);
        return entity.Id;
    }

    public async Task<Guid> AddPasswordResetAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var entity = new EmailDelivery
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = EmailDeliveryTypeEnum.PasswordReset,
            Status = DeliveryStatusEnum.Pending,
            CreatedAt = now,
            NextAttemptAt = now
        };

        await context.EmailDeliveries.AddAsync(entity, cancellationToken);
        return entity.Id;
    }

    public async Task RemoveUnsentAsync(
        Guid userId,
        EmailDeliveryTypeEnum type,
        CancellationToken cancellationToken)
    {
        var deliveries = await context.EmailDeliveries
            .Where(x => x.UserId == userId &&
                        x.Type == type &&
                        x.Status != DeliveryStatusEnum.Processed)
            .ToListAsync(cancellationToken);

        context.EmailDeliveries.RemoveRange(deliveries);
    }

    public async Task<List<EmailDelivery>> ClaimPendingAsync(
        int batchSize,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        return await context.EmailDeliveries
            .AsNoTracking()
            .Where(x => x.Status == DeliveryStatusEnum.Pending && x.NextAttemptAt <= now)
            .OrderBy(x => x.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public Task<EmailDelivery?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        return context.EmailDeliveries
            .AsNoTracking()
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task SetPasswordResetTokenAsync(
        Guid id,
        string token,
        DateTime expiresAt,
        CancellationToken cancellationToken)
    {
        var delivery = await context.EmailDeliveries
            .FirstAsync(x => x.Id == id, cancellationToken);

        delivery.PasswordResetToken = token;
        delivery.PasswordResetTokenExpiresAt = expiresAt;

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkProcessedAsync(Guid id, CancellationToken cancellationToken)
    {
        var delivery = await context.EmailDeliveries
            .FirstAsync(x => x.Id == id, cancellationToken);

        delivery.Status = DeliveryStatusEnum.Processed;
        delivery.ProcessedAt = DateTime.UtcNow;
        delivery.LastError = null;
        delivery.TemporaryPassword = null;
        delivery.PasswordResetToken = null;
        delivery.PasswordResetTokenExpiresAt = null;

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkRetryAsync(
        Guid id,
        int attemptCount,
        DateTime nextAttemptAt,
        string error,
        CancellationToken cancellationToken)
    {
        var delivery = await context.EmailDeliveries
            .FirstAsync(x => x.Id == id, cancellationToken);

        delivery.AttemptCount = attemptCount;
        delivery.NextAttemptAt = nextAttemptAt;
        delivery.LastError = error.Length > 2000 ? error[..2000] : error;

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkFailedAsync(
        Guid id,
        int attemptCount,
        string error,
        CancellationToken cancellationToken)
    {
        var delivery = await context.EmailDeliveries
            .FirstAsync(x => x.Id == id, cancellationToken);

        delivery.Status = DeliveryStatusEnum.Failed;
        delivery.AttemptCount = attemptCount;
        delivery.FailedAt = DateTime.UtcNow;
        delivery.LastError = error.Length > 2000 ? error[..2000] : error;
        delivery.TemporaryPassword = null;
        delivery.PasswordResetToken = null;
        delivery.PasswordResetTokenExpiresAt = null;

        await context.SaveChangesAsync(cancellationToken);
    }

    public Task DeleteProcessedBeforeAsync(
        DateTime processedBefore,
        CancellationToken cancellationToken)
    {
        return context.EmailDeliveries
            .Where(x => x.Status == DeliveryStatusEnum.Processed &&
                        x.ProcessedAt < processedBefore)
            .ExecuteDeleteAsync(cancellationToken);
    }

}

using ATMS.Data.Enums;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Models.Notifications;
using ATMS.Project.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ATMS.Project.Data.Repositories;

public sealed class EmailDeliveryRepository(ProjectDbContext context) : IEmailDeliveryRepository
{
    public Task AddRangeAsync(IEnumerable<EmailDelivery> deliveries, CancellationToken cancellationToken) =>
        context.EmailDeliveries.AddRangeAsync(deliveries, cancellationToken);

    public Task<EmailDelivery[]> ClaimPendingAsync(int batchSize, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        return context.EmailDeliveries
            .AsNoTracking()
            .Where(delivery => delivery.Status == (int)DeliveryStatusEnum.Pending && delivery.NextAttemptAt <= now)
            .OrderBy(delivery => delivery.CreatedAt)
            .Take(batchSize)
            .ToArrayAsync(cancellationToken);
    }

    public Task<EmailDeliveryRow?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        (from delivery in context.EmailDeliveries.AsNoTracking()
         where delivery.Id == id
         join user in context.Users on delivery.Notification.UserId equals user.Id into users
         from user in users.DefaultIfEmpty()
         select new EmailDeliveryRow(
             delivery.Id,
             delivery.Status,
             delivery.NotificationId,
             delivery.Notification.UserId,
             user == null ? null : user.Email,
             user == null ? null : user.Name,
             user == null ? null : user.Surname,
             user != null && user.IsActive,
             user == null ? null : user.Language))
        .FirstOrDefaultAsync(cancellationToken);

    public async Task MarkProcessedAsync(Guid id, CancellationToken cancellationToken)
    {
        var delivery = await context.EmailDeliveries.FirstAsync(row => row.Id == id, cancellationToken);

        delivery.Status = (int)DeliveryStatusEnum.Processed;
        delivery.ProcessedAt = DateTime.UtcNow;
        delivery.LastError = null;

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkRetryAsync(
        Guid id,
        int attemptCount,
        DateTime nextAttemptAt,
        string error,
        CancellationToken cancellationToken)
    {
        var delivery = await context.EmailDeliveries.FirstAsync(row => row.Id == id, cancellationToken);

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
        var delivery = await context.EmailDeliveries.FirstAsync(row => row.Id == id, cancellationToken);

        delivery.Status = (int)DeliveryStatusEnum.Failed;
        delivery.AttemptCount = attemptCount;
        delivery.FailedAt = DateTime.UtcNow;
        delivery.LastError = error.Length > 2000 ? error[..2000] : error;

        await context.SaveChangesAsync(cancellationToken);
    }
}

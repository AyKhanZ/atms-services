using ATMS.Data.Messaging;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ATMS.Project.Data.Repositories;

public class InboxRepository(ProjectDbContext context) : IInboxRepository
{
    public Task<bool> IsProcessedAsync(
        Guid messageId,
        string consumerName,
        CancellationToken cancellationToken)
    {
        return context.InboxMessages.AnyAsync(
            x => x.MessageId == messageId && x.ConsumerName == consumerName,
            cancellationToken);
    }

    public async Task AddAsync(
        Guid messageId,
        string consumerName,
        CancellationToken cancellationToken)
    {
        await context.InboxMessages.AddAsync(new InboxMessage
        {
            MessageId = messageId,
            ConsumerName = consumerName,
            ProcessedAt = DateTime.UtcNow
        }, cancellationToken);
    }

    public Task DeleteProcessedBeforeAsync(
        DateTime processedBefore,
        CancellationToken cancellationToken)
    {
        return context.InboxMessages
            .Where(x => x.ProcessedAt < processedBefore)
            .ExecuteDeleteAsync(cancellationToken);
    }
}

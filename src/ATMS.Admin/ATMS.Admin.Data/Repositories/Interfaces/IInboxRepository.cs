namespace ATMS.Admin.Data.Repositories.Interfaces;

public interface IInboxRepository
{
    Task<bool> IsProcessedAsync(
        Guid messageId,
        string consumerName,
        CancellationToken cancellationToken);

    Task AddAsync(
        Guid messageId,
        string consumerName,
        CancellationToken cancellationToken);

    Task DeleteProcessedBeforeAsync(
        DateTime processedBefore,
        CancellationToken cancellationToken);
}

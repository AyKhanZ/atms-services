namespace ATMS.Admin.Service.Infrastructure.Delivery;

public sealed class EmailDeliveryRequestLock
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public async Task ExecuteAsync(Func<Task> action, CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            await action();
        }
        finally
        {
            _semaphore.Release();
        }
    }
}

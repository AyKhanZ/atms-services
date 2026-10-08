using ATMS.Admin.Service.Infrastructure.Delivery;

namespace Admin.Services.Tests.Infrastructure.Delivery;

public class EmailDeliveryRequestLockTest
{
    [Fact]
    public async Task ExecuteAsync_WhenFirstRequestIsRunning_WaitsForItToFinish()
    {
        var requestLock = new EmailDeliveryRequestLock();
        var firstRequestStarted = new TaskCompletionSource();
        var releaseFirstRequest = new TaskCompletionSource();
        var secondRequestStarted = new TaskCompletionSource();

        var firstRequest = requestLock.ExecuteAsync(async () =>
        {
            firstRequestStarted.SetResult();
            await releaseFirstRequest.Task;
        }, CancellationToken.None);

        await firstRequestStarted.Task;

        var secondRequest = requestLock.ExecuteAsync(() =>
        {
            secondRequestStarted.SetResult();
            return Task.CompletedTask;
        }, CancellationToken.None);

        Assert.False(secondRequestStarted.Task.IsCompleted);

        releaseFirstRequest.SetResult();

        await Task.WhenAll(firstRequest, secondRequest);

        Assert.True(secondRequestStarted.Task.IsCompleted);
    }
}

using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Infrastructure.Delivery;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Admin.Services.Tests.Infrastructure.Delivery;

public class DeliveryRetentionBackgroundServiceTest
{
    [Fact]
    public async Task DeleteExpiredRecordsAsync_UsesConfiguredRetentionPeriods()
    {
        var outboxRepository = new Mock<IOutboxRepository>();
        var emailRepository = new Mock<IEmailDeliveryRepository>();
        var inboxRepository = new Mock<IInboxRepository>();
        var services = new ServiceCollection()
            .AddSingleton(outboxRepository.Object)
            .AddSingleton(emailRepository.Object)
            .AddSingleton(inboxRepository.Object)
            .BuildServiceProvider();
        var worker = new TestDeliveryRetentionBackgroundService(
            services.GetRequiredService<IServiceScopeFactory>());
        var now = DateTime.UtcNow;

        await worker.DeleteExpiredRecordsOnceAsync(CancellationToken.None);

        outboxRepository.Verify(
            x => x.DeleteProcessedBeforeAsync(
                It.Is<DateTime>(date => date >= now.AddDays(-30).AddSeconds(-1) &&
                                        date <= now.AddDays(-30).AddSeconds(1)),
                It.IsAny<CancellationToken>()),
            Times.Once);
        emailRepository.Verify(
            x => x.DeleteProcessedBeforeAsync(
                It.Is<DateTime>(date => date >= now.AddDays(-30).AddSeconds(-1) &&
                                        date <= now.AddDays(-30).AddSeconds(1)),
                It.IsAny<CancellationToken>()),
            Times.Once);
        inboxRepository.Verify(
            x => x.DeleteProcessedBeforeAsync(
                It.Is<DateTime>(date => date >= now.AddDays(-60).AddSeconds(-1) &&
                                        date <= now.AddDays(-60).AddSeconds(1)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private sealed class TestDeliveryRetentionBackgroundService(
        IServiceScopeFactory scopeFactory)
        : DeliveryRetentionBackgroundService(
            scopeFactory,
            NullLogger<DeliveryRetentionBackgroundService>.Instance)
    {
        public Task DeleteExpiredRecordsOnceAsync(CancellationToken cancellationToken)
        {
            return DeleteExpiredRecordsAsync(cancellationToken);
        }
    }
}

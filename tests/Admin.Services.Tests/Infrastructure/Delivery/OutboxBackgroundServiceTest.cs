using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Infrastructure.Delivery;
using ATMS.Data.Messaging;
using ATMS.Messaging.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Admin.Services.Tests.Infrastructure.Delivery;

public class OutboxBackgroundServiceTest
{
    [Fact]
    public async Task ProcessBatchAsync_WhenPublishSucceeds_MarksMessageProcessed()
    {
        var message = CreateMessage();
        var repository = new Mock<IOutboxRepository>();
        var publisher = new Mock<IMessagePublisher>();
        repository
            .Setup(x => x.ClaimPendingAsync(20, It.IsAny<CancellationToken>()))
            .ReturnsAsync([message]);
        publisher
            .Setup(x => x.PublishAsync(
                message.Exchange,
                message.RoutingKey,
                message.MessageType,
                message.Payload,
                message.Id,
                message.CreatedAt,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var worker = CreateWorker(repository.Object, publisher.Object);

        var count = await worker.ProcessOnceAsync(CancellationToken.None);

        Assert.Equal(1, count);
        repository.Verify(
            x => x.MarkProcessedAsync(message.Id, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessBatchAsync_WhenPublishFails_SchedulesRetry()
    {
        var message = CreateMessage();
        var repository = new Mock<IOutboxRepository>();
        var publisher = new Mock<IMessagePublisher>();
        repository
            .Setup(x => x.ClaimPendingAsync(20, It.IsAny<CancellationToken>()))
            .ReturnsAsync([message]);
        publisher
            .Setup(x => x.PublishAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("RabbitMQ unavailable"));
        var worker = CreateWorker(repository.Object, publisher.Object);

        await worker.ProcessOnceAsync(CancellationToken.None);

        repository.Verify(
            x => x.MarkRetryAsync(
                message.Id,
                1,
                It.Is<DateTime>(date => date > DateTime.UtcNow),
                "RabbitMQ unavailable",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessBatchAsync_WhenProcessedStateCannotBeSaved_SchedulesRetry()
    {
        var message = CreateMessage();
        var repository = new Mock<IOutboxRepository>();
        var publisher = new Mock<IMessagePublisher>();
        repository
            .Setup(x => x.ClaimPendingAsync(20, It.IsAny<CancellationToken>()))
            .ReturnsAsync([message]);
        repository
            .Setup(x => x.MarkProcessedAsync(message.Id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database unavailable"));
        publisher
            .Setup(x => x.PublishAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var worker = CreateWorker(repository.Object, publisher.Object);

        await worker.ProcessOnceAsync(CancellationToken.None);

        repository.Verify(
            x => x.MarkRetryAsync(
                message.Id,
                1,
                It.Is<DateTime>(date => date > DateTime.UtcNow),
                "Database unavailable",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessBatchAsync_WhenTenthAttemptFails_MarksMessageFailed()
    {
        var message = CreateMessage();
        message.AttemptCount = 9;
        var repository = new Mock<IOutboxRepository>();
        var publisher = new Mock<IMessagePublisher>();
        repository
            .Setup(x => x.ClaimPendingAsync(20, It.IsAny<CancellationToken>()))
            .ReturnsAsync([message]);
        publisher
            .Setup(x => x.PublishAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("RabbitMQ unavailable"));
        var worker = CreateWorker(repository.Object, publisher.Object);

        await worker.ProcessOnceAsync(CancellationToken.None);

        repository.Verify(
            x => x.MarkFailedAsync(
                message.Id,
                10,
                "RabbitMQ unavailable",
                It.IsAny<CancellationToken>()),
            Times.Once);
        repository.Verify(
            x => x.MarkRetryAsync(
                It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<DateTime>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private TestOutboxBackgroundService CreateWorker(
        IOutboxRepository repository,
        IMessagePublisher publisher)
    {
        var services = new ServiceCollection()
            .AddSingleton(repository)
            .AddSingleton(publisher)
            .BuildServiceProvider();

        return new TestOutboxBackgroundService(
            services.GetRequiredService<IServiceScopeFactory>(),
            new DeliveryRetrySchedule());
    }

    private OutboxMessage CreateMessage()
    {
        return new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Exchange = "events",
            RoutingKey = "user.created",
            MessageType = "UserCreatedEvent",
            Payload = "{}",
            CreatedAt = DateTime.UtcNow,
            NextAttemptAt = DateTime.UtcNow
        };
    }

    private sealed class TestOutboxBackgroundService(
        IServiceScopeFactory scopeFactory,
        DeliveryRetrySchedule retrySchedule)
        : OutboxBackgroundService(
            scopeFactory,
            retrySchedule,
            NullLogger<OutboxBackgroundService>.Instance)
    {
        public Task<int> ProcessOnceAsync(CancellationToken cancellationToken)
        {
            return ProcessBatchAsync(cancellationToken);
        }
    }
}

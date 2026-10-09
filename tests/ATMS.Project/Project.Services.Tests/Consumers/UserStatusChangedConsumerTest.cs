using Microsoft.Extensions.Options;
using System.Linq.Expressions;
using ATMS.Application.Localization;
using ATMS.Caching.Constants;
using ATMS.Caching.Services.Interfaces;
using ATMS.Contracts.Events.Users;
using ATMS.Infrastructure.Options;
using ATMS.Messaging.Infrastructure;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Consumers.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Project.Services.Tests.Consumers;

public class UserStatusChangedConsumerTest
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IInboxRepository> _inboxRepositoryMock = new();
    private readonly Mock<IWorkProjectRepository> _workProjectRepositoryMock = new();
    private readonly Mock<ICacheService> _cacheMock = new();
    private readonly User _user = new() { Id = Guid.NewGuid(), IsActive = true };

    public UserStatusChangedConsumerTest()
    {
        _userRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_user);
    }

    [Fact]
    public async Task HandleAsync_CopiesTheFlagAndDropsCachedProjects()
    {
        Guid[] projectIds = [Guid.NewGuid(), Guid.NewGuid()];
        _workProjectRepositoryMock
            .Setup(x => x.GetIdsByParticipantAsync(_user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(projectIds);

        await RunAsync(new UserStatusChangedEvent(_user.Id, false));

        Assert.False(_user.IsActive);
        _userRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
        foreach (var projectId in projectIds)
        {
            foreach (var language in SupportedLanguages.All)
            {
                _cacheMock.Verify(
                    x => x.RemoveAsync(CacheKeys.Project.ProjectById(projectId, language), It.IsAny<CancellationToken>()),
                    Times.Once);
            }
        }
    }

    [Fact]
    public async Task HandleAsync_WhenMessageWasProcessed_TouchesNothing()
    {
        _inboxRepositoryMock
            .Setup(x => x.IsProcessedAsync(It.IsAny<Guid>(), nameof(UserStatusChangedConsumer), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await RunAsync(new UserStatusChangedEvent(_user.Id, false));

        _userRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
        _cacheMock.Verify(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private Task RunAsync(UserStatusChangedEvent message)
    {
        var services = new ServiceCollection()
            .AddSingleton(_userRepositoryMock.Object)
            .AddSingleton(_inboxRepositoryMock.Object)
            .AddSingleton(_workProjectRepositoryMock.Object)
            .AddSingleton(_cacheMock.Object)
            .BuildServiceProvider();
        var consumer = new TestUserStatusChangedConsumer(
            new RabbitMqConnectionFactory(Options.Create(CreateQueueConfiguration().GetSection(nameof(QueueOptions)).Get<QueueOptions>()!)),
            services.GetRequiredService<IServiceScopeFactory>());

        return consumer.RunAsync(message, Guid.NewGuid(), services);
    }

    private static IConfiguration CreateQueueConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{nameof(QueueOptions)}:{nameof(QueueOptions.Host)}"] = "localhost",
                [$"{nameof(QueueOptions)}:{nameof(QueueOptions.Username)}"] = "guest",
                [$"{nameof(QueueOptions)}:{nameof(QueueOptions.Password)}"] = "guest",
                [$"{nameof(QueueOptions)}:{nameof(QueueOptions.Port)}"] = "5672",
                [$"{nameof(QueueOptions)}:{nameof(QueueOptions.VirtualHost)}"] = "/"
            })
            .Build();
    }

    private sealed class TestUserStatusChangedConsumer(
        RabbitMqConnectionFactory connectionFactory,
        IServiceScopeFactory scopeFactory)
        : UserStatusChangedConsumer(connectionFactory, scopeFactory, NullLogger<UserStatusChangedConsumer>.Instance)
    {
        public Task RunAsync(UserStatusChangedEvent message, Guid messageId, IServiceProvider serviceProvider)
        {
            return HandleAsync(message, messageId, serviceProvider, CancellationToken.None);
        }
    }
}

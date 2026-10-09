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
using ATMS.Project.Services.Modules;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Project.Services.Tests.Consumers;

public class UserUpdatedConsumerTest
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IInboxRepository> _inboxRepositoryMock = new();
    private readonly Mock<IWorkProjectRepository> _workProjectRepositoryMock = new();
    private readonly Mock<ICacheService> _cacheMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly User _user = new() { Id = Guid.NewGuid(), HasCompletedOnboarding = false };

    public UserUpdatedConsumerTest()
    {
        _userRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_user);
    }

    // The participant row in project details carries the onboarding flag; a cached copy kept Invited
    // on screen for up to five minutes after the person finished onboarding.
    [Fact]
    public async Task HandleAsync_FlushesDetailsOfEveryProjectTheUserTakesPartIn()
    {
        Guid[] projectIds = [Guid.NewGuid(), Guid.NewGuid()];
        _workProjectRepositoryMock
            .Setup(x => x.GetIdsByParticipantAsync(_user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(projectIds);

        await RunAsync(new UserUpdatedEvent(_user.Id, "Nigar", "Huseynova", "avatar.png", true));

        _mapperMock.Verify(x => x.Map(It.IsAny<UserUpdatedEvent>(), _user), Times.Once);
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
            .Setup(x => x.IsProcessedAsync(It.IsAny<Guid>(), nameof(UserUpdatedConsumer), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await RunAsync(new UserUpdatedEvent(_user.Id, "Nigar", "Huseynova", "avatar.png", true));

        _userRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
        _cacheMock.Verify(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_SavesTheLanguageFromTheEvent()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMapperServices();
        services.AddSingleton(_userRepositoryMock.Object);
        services.AddSingleton(_inboxRepositoryMock.Object);
        services.AddSingleton(_workProjectRepositoryMock.Object);
        services.AddSingleton(_cacheMock.Object);
        var provider = services.BuildServiceProvider();
        var consumer = new TestUserUpdatedConsumer(
            new RabbitMqConnectionFactory(Options.Create(CreateQueueConfiguration().GetSection(nameof(QueueOptions)).Get<QueueOptions>()!)),
            provider.GetRequiredService<IServiceScopeFactory>());

        await consumer.RunAsync(
            new UserUpdatedEvent(_user.Id, "Nigar", "Huseynova", "avatar.png", true, Language: "az"),
            Guid.NewGuid(),
            provider);

        Assert.Equal("az", _user.Language);
        _userRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private Task RunAsync(UserUpdatedEvent message)
    {
        var services = new ServiceCollection()
            .AddSingleton(_userRepositoryMock.Object)
            .AddSingleton(_inboxRepositoryMock.Object)
            .AddSingleton(_workProjectRepositoryMock.Object)
            .AddSingleton(_cacheMock.Object)
            .AddSingleton(_mapperMock.Object)
            .BuildServiceProvider();
        var consumer = new TestUserUpdatedConsumer(
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

    private sealed class TestUserUpdatedConsumer(
        RabbitMqConnectionFactory connectionFactory,
        IServiceScopeFactory scopeFactory)
        : UserUpdatedConsumer(connectionFactory, scopeFactory, NullLogger<UserUpdatedConsumer>.Instance)
    {
        public Task RunAsync(UserUpdatedEvent message, Guid messageId, IServiceProvider serviceProvider)
        {
            return HandleAsync(message, messageId, serviceProvider, CancellationToken.None);
        }
    }
}

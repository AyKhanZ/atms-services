using Microsoft.Extensions.Options;
using System.Linq.Expressions;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Entities.Onboarding;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Consumers.Users;
using ATMS.Admin.Service.Security.Interfaces;
using ATMS.Contracts.Events.Users;
using ATMS.Data.Constants;
using ATMS.Data.Enums;
using ATMS.Data.Messaging;
using ATMS.Infrastructure.Options;
using ATMS.Messaging.Configuration;
using ATMS.Messaging.Infrastructure;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Admin.Services.Tests.Consumers;

public class UserInvitedConsumerTest
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IRoleRepository> _roleRepositoryMock = new();
    private readonly Mock<IPasswordService> _passwordServiceMock = new();
    private readonly Mock<IPasswordHasherService> _passwordHasherMock = new();
    private readonly Mock<IInboxRepository> _inboxRepositoryMock = new();
    private readonly Mock<IOutboxRepository> _outboxRepositoryMock = new();
    private readonly Mock<IEmailDeliveryRepository> _emailDeliveryRepositoryMock = new();
    private readonly Mock<IOnboardingRepository> _onboardingRepositoryMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly User _inviter = new()
    {
        Id = Guid.NewGuid(),
        Name = "Leyla",
        Surname = "Mammadova"
    };

    public UserInvitedConsumerTest()
    {
        _roleRepositoryMock
            .Setup(x => x.GetAsync(It.IsAny<Expression<Func<Role, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Role { Id = RoleIds.Client, UserType = (int)UserTypeEnum.Client });
        _passwordServiceMock.Setup(x => x.GenerateRandomPassword()).Returns("Temporary1!");
        _passwordHasherMock.Setup(x => x.Hash("Temporary1!")).Returns("hash");
        _mapperMock
            .Setup(x => x.Map<User>(It.IsAny<UserInvitedEvent>()))
            .Returns<UserInvitedEvent>(message => new User
            {
                Email = message.Email,
                Name = message.Name,
                Surname = message.Surname,
                OrganizationId = message.OrganizationId,
                AvatarPath = "avatar.png"
            });
    }

    [Theory]
    [InlineData(null)]
    [InlineData("Customer portal")]
    public async Task HandleAsync_WhenEmailIsNew_QueuesInvitationEmailWithInviterAndProject(string? projectTitle)
    {
        SetupExistingUser(null);
        SetupInviter();
        var message = CreateMessage(projectTitle);

        await RunAsync(message);

        _emailDeliveryRepositoryMock.Verify(x => x.AddInvitationAsync(
            It.IsAny<Guid>(),
            "Temporary1!",
            "Leyla Mammadova",
            projectTitle,
            It.IsAny<CancellationToken>()), Times.Once);
        _outboxRepositoryMock.Verify(x => x.AddAsync(
            MessagingConstants.Exchanges.UserEvents,
            MessagingConstants.RoutingKeys.UserCreated,
            It.Is<UserCreatedEvent>(e => e.Email == message.Email && !e.HasCompletedOnboarding),
            It.IsAny<CancellationToken>()), Times.Once);
        _onboardingRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<OnboardingProgress>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenInviterIsGone_SendsEmailWithoutInviterLine()
    {
        SetupExistingUser(null);

        await RunAsync(CreateMessage("Customer portal"));

        _emailDeliveryRepositoryMock.Verify(x => x.AddInvitationAsync(
            It.IsAny<Guid>(),
            "Temporary1!",
            null,
            "Customer portal",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // A project invitation that passed Project's check a moment before the email was registered must
    // still hear back, or it waits forever.
    [Fact]
    public async Task HandleAsync_WhenEmailAlreadyExists_AnnouncesExistingUserAndSendsNoEmail()
    {
        var existing = new User
        {
            Id = Guid.NewGuid(),
            Email = "Nigar@Client.az",
            Name = "Nigar",
            Surname = "Huseynova",
            AvatarPath = "nigar.png",
            OrganizationId = Guid.NewGuid(),
            HasCompletedOnboarding = true,
            Position = "Procurement lead"
        };
        SetupExistingUser(existing);
        _userRepositoryMock
            .Setup(x => x.GetRolesAsync(existing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new Role { Id = RoleIds.Client, UserType = (int)UserTypeEnum.Client }]);

        await RunAsync(CreateMessage("Customer portal"));

        _outboxRepositoryMock.Verify(x => x.AddAsync(
            MessagingConstants.Exchanges.UserEvents,
            MessagingConstants.RoutingKeys.UserCreated,
            new UserCreatedEvent(
                existing.Id,
                existing.Email,
                existing.Name,
                existing.Surname,
                (int)UserTypeEnum.Client,
                existing.AvatarPath,
                existing.OrganizationId,
                false,
                true,
                "Procurement lead"),
            It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _emailDeliveryRepositoryMock.Verify(x => x.AddInvitationAsync(
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _inboxRepositoryMock.Verify(x => x.AddAsync(
            It.IsAny<Guid>(),
            nameof(UserInvitedConsumer),
            It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // Broken data must not send the message to the dead-letter queue: it is logged and marked processed.
    [Fact]
    public async Task HandleAsync_WhenExistingUserHasNoRole_SkipsAnnouncementWithoutFailing()
    {
        var existing = new User { Id = Guid.NewGuid(), Email = "Nigar@Client.az" };
        SetupExistingUser(existing);
        _userRepositoryMock
            .Setup(x => x.GetRolesAsync(existing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        await RunAsync(CreateMessage("Customer portal"));

        _outboxRepositoryMock.Verify(x => x.AddAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<UserCreatedEvent>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _inboxRepositoryMock.Verify(x => x.AddAsync(
            It.IsAny<Guid>(),
            nameof(UserInvitedConsumer),
            It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenMessageWasProcessed_DoesNothing()
    {
        _inboxRepositoryMock
            .Setup(x => x.IsProcessedAsync(It.IsAny<Guid>(), nameof(UserInvitedConsumer), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await RunAsync(CreateMessage(null));

        _userRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
        _outboxRepositoryMock.Verify(x => x.AddAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<UserCreatedEvent>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    private void SetupExistingUser(User? user)
    {
        _userRepositoryMock
            .Setup(x => x.FindAsync(
                It.Is<Expression<Func<User, bool>>>(predicate => IsEmailLookup(predicate)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
    }

    private void SetupInviter()
    {
        _userRepositoryMock
            .Setup(x => x.FindAsync(
                It.Is<Expression<Func<User, bool>>>(predicate => predicate.Compile()(_inviter)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_inviter);
    }

    // The email lookup matches by normalized email, so trimming and case do not create a second account.
    private static bool IsEmailLookup(Expression<Func<User, bool>> predicate)
    {
        return predicate.Compile()(new User { Id = Guid.NewGuid(), NormalizedEmail = "NIGAR@CLIENT.AZ" });
    }

    private UserInvitedEvent CreateMessage(string? projectTitle)
    {
        return new UserInvitedEvent(" Nigar@Client.az ", "Nigar", "Huseynova", Guid.NewGuid(), _inviter.Id, projectTitle);
    }

    private Task RunAsync(UserInvitedEvent message)
    {
        var services = new ServiceCollection()
            .AddSingleton(_userRepositoryMock.Object)
            .AddSingleton(_roleRepositoryMock.Object)
            .AddSingleton(_passwordServiceMock.Object)
            .AddSingleton(_passwordHasherMock.Object)
            .AddSingleton(_inboxRepositoryMock.Object)
            .AddSingleton(_outboxRepositoryMock.Object)
            .AddSingleton(_emailDeliveryRepositoryMock.Object)
            .AddSingleton(_onboardingRepositoryMock.Object)
            .AddSingleton(_mapperMock.Object)
            .BuildServiceProvider();
        var consumer = new TestUserInvitedConsumer(
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

    private sealed class TestUserInvitedConsumer(
        RabbitMqConnectionFactory connectionFactory,
        IServiceScopeFactory scopeFactory)
        : UserInvitedConsumer(connectionFactory, scopeFactory, NullLogger<UserInvitedConsumer>.Instance)
    {
        public Task RunAsync(UserInvitedEvent message, Guid messageId, IServiceProvider serviceProvider)
        {
            return HandleAsync(message, messageId, serviceProvider, CancellationToken.None);
        }
    }
}

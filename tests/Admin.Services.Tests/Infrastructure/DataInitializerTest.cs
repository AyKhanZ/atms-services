using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Infrastructure;
using ATMS.Admin.Service.Security.Interfaces;
using ATMS.Contracts.Events.Users;
using ATMS.Data.Constants;
using ATMS.Data.Enums;
using ATMS.Infrastructure.Options;
using ATMS.Messaging.Configuration;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Admin.Services.Tests.Infrastructure;

public sealed class DataInitializerTest
{
    private const string Email = "admin@atms.local";
    private const string Name = "System";
    private const string Surname = "Administrator";

    private readonly Mock<IUserRepository> userRepository = new();
    private readonly Mock<IRoleRepository> roleRepository = new();
    private readonly Mock<IPasswordHasherService> passwordHasher = new();
    private readonly Mock<IOutboxRepository> outboxRepository = new();

    [Fact]
    public async Task InitializeAsync_WhenSuperAdminAlreadyExists_QueuesMissingUserCreatedEvent()
    {
        var user = CreateUser();
        SetupRole();
        userRepository
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        outboxRepository
            .Setup(x => x.ContainsAsync(
                MessagingConstants.Exchanges.UserEvents,
                MessagingConstants.RoutingKeys.UserCreated,
                It.IsAny<UserCreatedEvent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await CreateInitializer().InitializeAsync();

        outboxRepository.Verify(x => x.AddAsync(
            MessagingConstants.Exchanges.UserEvents,
            MessagingConstants.RoutingKeys.UserCreated,
            It.Is<UserCreatedEvent>(message =>
                message.Id == user.Id &&
                message.Email == user.Email &&
                message.UserType == (int)UserTypeEnum.SuperAdmin &&
                message.IsAdmin),
            It.IsAny<CancellationToken>()), Times.Once);
        userRepository.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task InitializeAsync_WhenProjectionEventAlreadyExists_DoesNotQueueDuplicate()
    {
        SetupRole();
        userRepository
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateUser());
        outboxRepository
            .Setup(x => x.ContainsAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<UserCreatedEvent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await CreateInitializer().InitializeAsync();

        outboxRepository.Verify(x => x.AddAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<UserCreatedEvent>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task InitializeAsync_WhenSuperAdminDoesNotExist_CreatesUserAndQueuesProjectionEvent()
    {
        SetupRole();
        passwordHasher.Setup(x => x.Hash("password")).Returns("password-hash");
        userRepository
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        outboxRepository
            .Setup(x => x.ContainsAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<UserCreatedEvent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await CreateInitializer().InitializeAsync();

        userRepository.Verify(x => x.AddAsync(
            It.Is<User>(user =>
                user.Email == Email &&
                user.PasswordHash == "password-hash" &&
                user.AvatarPath == DefaultValues.UserAvatar &&
                user.UserRoles.Single().RoleId == RoleIds.SuperAdmin),
            It.IsAny<CancellationToken>()), Times.Once);
        outboxRepository.Verify(x => x.AddAsync(
            MessagingConstants.Exchanges.UserEvents,
            MessagingConstants.RoutingKeys.UserCreated,
            It.IsAny<UserCreatedEvent>(),
            It.IsAny<CancellationToken>()), Times.Once);
        userRepository.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private DataInitializer CreateInitializer() =>
        new(
            BuildConfiguration(),
            userRepository.Object,
            roleRepository.Object,
            passwordHasher.Object,
            outboxRepository.Object);

    private void SetupRole()
    {
        roleRepository
            .Setup(x => x.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Role, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Role
            {
                Id = RoleIds.SuperAdmin,
                Name = "SuperAdmin",
                UserType = (int)UserTypeEnum.SuperAdmin
            });
    }

    private static User CreateUser() =>
        new()
        {
            Id = Guid.NewGuid(),
            Email = Email,
            NormalizedEmail = Email.ToUpperInvariant(),
            Name = Name,
            Surname = Surname,
            AvatarPath = DefaultValues.UserAvatar,
            PasswordHash = "hash",
            IsAdmin = true
        };

    private static IConfiguration BuildConfiguration() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{nameof(AdminOptions)}:{nameof(AdminOptions.Email)}"] = Email,
                [$"{nameof(AdminOptions)}:{nameof(AdminOptions.Name)}"] = Name,
                [$"{nameof(AdminOptions)}:{nameof(AdminOptions.Surname)}"] = Surname,
                [$"{nameof(AdminOptions)}:{nameof(AdminOptions.Password)}"] = "password",
                [$"{nameof(AdminOptions)}:{nameof(AdminOptions.RoleName)}"] = "SuperAdmin"
            })
            .Build();
}

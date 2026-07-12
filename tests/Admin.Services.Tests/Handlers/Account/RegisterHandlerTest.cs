using System.Linq.Expressions;
using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Contracts.Models.Users;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Service.Handlers.Account;
using ATMS.Admin.Service.Security.Models;
using ATMS.Application.Exceptions.Configuration;
using ATMS.Email.Models;
using ATMS.Contracts.Events.Users;
using ATMS.Messaging.Configuration;
using Moq;

namespace Admin.Services.Tests.Handlers.Account;

public class RegisterHandlerTest : BaseHandlerTest
{
    private readonly RegisterHandler _handler;

    private const string FakePassword = "RandPass1!";
    private const string FakePasswordHash = "hashed-password";
    private const string FakeToken = "fake-email-token";

    public RegisterHandlerTest()
    {
        _handler = new RegisterHandler(
            UserRepositoryMock.Object,
            RoleRepositoryMock.Object,
            CurrentUserMock.Object,
            MapperMock.Object,
            PasswordServiceMock.Object,
            PasswordHasherServiceMock.Object,
            EmailConfirmationTokenServiceMock.Object,
            EmailSenderMock.Object,
            MessagePublisherMock.Object,
            BuildConfiguration());

        PasswordServiceMock
            .Setup(p => p.GenerateRandomPassword())
            .Returns(FakePassword);

        PasswordHasherServiceMock
            .Setup(p => p.Hash(FakePassword))
            .Returns(FakePasswordHash);

        EmailConfirmationTokenServiceMock
            .Setup(s => s.GenerateToken(It.IsAny<User>()))
            .Returns(new EmailConfirmationTokenResult(FakeToken, DateTime.UtcNow.AddHours(24)));
    }

    private RegisterCommand CreateCommand(Guid? roleId = null, Guid? organizationId = null) =>
        new()
        {
            Email = Faker.Internet.Email(),
            Name = Faker.Name.FirstName(),
            Surname = Faker.Name.LastName(),
            RoleId = roleId ?? Guid.NewGuid(),
            OrganizationId = organizationId
        };

    private void SetupRole(Guid roleId) =>
        RoleRepositoryMock
            .Setup(r => r.GetAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Role { Id = roleId, Name = "Admin" });

    private void SetupMapper(RegisterCommand command, User entity)
    {
        MapperMock.Setup(m => m.Map<User>(command)).Returns(entity);
        MapperMock.Setup(m => m.Map<UserModel>(entity)).Returns(new UserModel { Id = entity.Id });
    }

    // -------------------------
    // Основные сценарии
    // -------------------------

    [Fact]
    public async Task Handle_WhenRoleExists_ReturnsMappedUserModel()
    {
        // Arrange
        var command = CreateCommand();
        var entity = new User { Id = Guid.NewGuid(), Email = command.Email, Name = command.Name, Surname = command.Surname };
        var expectedModel = new UserModel { Id = entity.Id };

        MapperMock.Setup(m => m.Map<User>(command)).Returns(entity);
        MapperMock.Setup(m => m.Map<UserModel>(entity)).Returns(expectedModel);
        SetupRole(command.RoleId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(expectedModel, result);
    }

    [Fact]
    public async Task Handle_WhenRoleExists_SetsHashedPassword()
    {
        // Arrange
        var command = CreateCommand();
        var entity = new User { Id = Guid.NewGuid() };

        SetupMapper(command, entity);
        SetupRole(command.RoleId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(FakePasswordHash, entity.PasswordHash);
    }

    [Fact]
    public async Task Handle_WhenRoleExists_CreatesUser()
    {
        // Arrange
        var command = CreateCommand();
        var entity = new User { Id = Guid.NewGuid() };

        SetupMapper(command, entity);
        SetupRole(command.RoleId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        UserRepositoryMock.Verify(r => r.CreateAsync(
            It.Is<User>(u => u.Id == entity.Id),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // -------------------------
    // Email
    // -------------------------

    [Fact]
    public async Task Handle_WhenRoleExists_SendsEmailWithCorrectLink()
    {
        // Arrange
        var command = CreateCommand();
        var entity = new User { Id = Guid.NewGuid(), Email = command.Email, Name = command.Name, Surname = command.Surname };

        SetupMapper(command, entity);
        SetupRole(command.RoleId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        EmailSenderMock.Verify(s => s.SendAsync(
            entity.Email,
            It.Is<InviteModel>(m => m.Link.Contains(FakeToken) && m.Link.Contains(BaseUrl)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRoleExists_SendsEmailWithCorrectUserData()
    {
        // Arrange
        var command = CreateCommand();
        var entity = new User { Id = Guid.NewGuid(), Email = command.Email, Name = command.Name, Surname = command.Surname };

        SetupMapper(command, entity);
        SetupRole(command.RoleId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        EmailSenderMock.Verify(s => s.SendAsync(
            entity.Email,
            It.Is<InviteModel>(m =>
                m.Email == entity.Email &&
                m.Name == entity.Name &&
                m.Surname == entity.Surname &&
                m.Password == FakePassword),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // -------------------------
    // Очередь сообщений
    // -------------------------

    [Fact]
    public async Task Handle_WhenRoleExists_PublishesUserCreatedEvent()
    {
        // Arrange
        var command = CreateCommand();
        var entity = new User { Id = Guid.NewGuid(), Email = command.Email, Name = command.Name, Surname = command.Surname };

        SetupMapper(command, entity);
        SetupRole(command.RoleId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        MessagePublisherMock.Verify(p => p.PublishAsync(
            MessagingConstants.Exchanges.UserEvents,
            MessagingConstants.RoutingKeys.UserCreated,
            It.Is<UserCreatedEvent>(e =>
                e.Id == entity.Id &&
                e.Email == entity.Email &&
                e.Name == entity.Name &&
                e.Surname == entity.Surname),
            It.IsAny<CancellationToken>()), Times.Once);
    }


    [Fact]
    public async Task Handle_WhenRoleExists_PublishesUserCreatedEvent_WithOrganizationId()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var command = CreateCommand(organizationId: organizationId);
        var entity = new User { Id = Guid.NewGuid(), Email = command.Email, Name = command.Name, Surname = command.Surname };

        SetupMapper(command, entity);
        SetupRole(command.RoleId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        MessagePublisherMock.Verify(p => p.PublishAsync(
            MessagingConstants.Exchanges.UserEvents,
            MessagingConstants.RoutingKeys.UserCreated,
            It.Is<UserCreatedEvent>(e => e.OrganizationId == organizationId),
            It.IsAny<CancellationToken>()), Times.Once);
    }
    [Fact]
    public async Task Handle_WhenRoleNotFound_DoesNotPublishEvent()
    {
        // Arrange
        var command = CreateCommand();
        var entity = new User { Id = Guid.NewGuid() };

        MapperMock.Setup(m => m.Map<User>(command)).Returns(entity);

        RoleRepositoryMock
            .Setup(r => r.GetAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        // Act
        try { await _handler.Handle(command, CancellationToken.None); }
        catch
        {
            // ignored
        }

        // Assert
        MessagePublisherMock.Verify(p => p.PublishAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<UserCreatedEvent>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    // -------------------------
    // Ошибки
    // -------------------------

    [Fact]
    public async Task Handle_WhenRoleNotFound_ThrowsConfigurationException()
    {
        // Arrange
        var command = CreateCommand();
        var entity = new User { Id = Guid.NewGuid() };

        MapperMock.Setup(m => m.Map<User>(command)).Returns(entity);

        RoleRepositoryMock
            .Setup(r => r.GetAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ConfigurationException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Equal(ConfigurationErrorType.MissingSeedData, exception.ErrorType);
    }
}
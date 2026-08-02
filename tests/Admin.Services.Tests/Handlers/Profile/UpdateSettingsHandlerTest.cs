using System.Linq.Expressions;
using ATMS.Admin.Contracts.Commands.Profile;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Service.Handlers.Profile;
using ATMS.Application.Exceptions.Entity;
using ATMS.Contracts.Events.Users;
using ATMS.Messaging.Configuration;
using Moq;

namespace Admin.Services.Tests.Handlers.Profile;

public class UpdateSettingsHandlerTest : BaseHandlerTest
{
    private readonly UpdateSettingsHandler _handler;

    public UpdateSettingsHandlerTest()
    {
        _handler = new UpdateSettingsHandler(
            UserRepositoryMock.Object,
            OutboxRepositoryMock.Object,
            CacheServiceMock.Object);
    }

    private User CreateUser() => new()
    {
        Id = Guid.NewGuid(),
        Name = "OldName",
        Surname = "OldSurname",
        PhoneNumber = "0000000000",
        AvatarPath = "avatar.jpg"
    };

    private UpdateSettingsCommand CreateCommand(Guid id) => new()
    {
        Id = id,
        Name = Faker.Name.FirstName(),
        Surname = Faker.Name.FirstName(),
        PhoneNumber = Faker.Phone.PhoneNumber(),
        BirthDate = Faker.Date.Recent(),
        Position = "Developer",
        MaritalStatusId = 1,
        GenderId = 1
    };

    [Fact]
    public async Task Handle_UpdatesUserFields()
    {
        // Arrange
        var user = CreateUser();
        var command = CreateCommand(user.Id);

        UserRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(command.Name, user.Name);
        Assert.Equal(command.Surname, user.Surname);
        Assert.Equal(command.PhoneNumber, user.PhoneNumber);
        Assert.Equal(command.BirthDate, user.BirthDate);
        Assert.Equal(command.Position, user.Position);
        Assert.Equal(command.MaritalStatusId, user.MaritalStatusId);
        Assert.Equal(command.GenderId, user.GenderId);
    }

    [Fact]
    public async Task Handle_SavesChanges()
    {
        // Arrange
        var user = CreateUser();
        var command = CreateCommand(user.Id);

        UserRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        UserRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_QueuesUserUpdatedEvent()
    {
        // Arrange
        var user = CreateUser();
        var command = CreateCommand(user.Id);

        UserRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        OutboxRepositoryMock.Verify(p => p.AddAsync(
            MessagingConstants.Exchanges.UserEvents,
            MessagingConstants.RoutingKeys.UserUpdated,
            It.Is<UserUpdatedEvent>(e => e.Id == user.Id && e.Name == command.Name && e.Surname == command.Surname),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ThrowsEntityException()
    {
        // Arrange
        var command = CreateCommand(Guid.NewGuid());

        UserRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<EntityException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }
}

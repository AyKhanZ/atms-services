using System.Linq.Expressions;
using ATMS.Admin.Contracts.Commands.Users;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Service.Handlers.Users;
using ATMS.Application.Exceptions.Entity;
using Moq;

namespace Admin.Services.Tests.Handlers.Users;

public class UpdateUserStatusHandlerTest : BaseHandlerTest
{
    private readonly UpdateUserStatusHandler _handler;

    public UpdateUserStatusHandlerTest()
    {
        _handler = new UpdateUserStatusHandler(
            UserRepositoryMock.Object,
            CacheServiceMock.Object);
    }

    private User CreateUser() => new() { Id = Guid.NewGuid(), UserStatusId = 1 };

    [Fact]
    public async Task Handle_UpdatesUserStatus()
    {
        // Arrange
        var user = CreateUser();
        var command = new UpdateUserStatusCommand { Id = user.Id, UserStatusId = 2 };

        UserRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(command.UserStatusId, user.UserStatusId);
    }

    [Fact]
    public async Task Handle_SavesChanges()
    {
        // Arrange
        var user = CreateUser();
        var command = new UpdateUserStatusCommand { Id = user.Id, UserStatusId = 2 };

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
    public async Task Handle_WhenUserNotFound_ThrowsEntityException()
    {
        // Arrange
        var command = new UpdateUserStatusCommand { Id = Guid.NewGuid(), UserStatusId = 2 };

        UserRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<EntityException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }
}
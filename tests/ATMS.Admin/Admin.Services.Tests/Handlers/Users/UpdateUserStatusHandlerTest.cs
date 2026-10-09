using System.Linq.Expressions;
using ATMS.Admin.Contracts.Commands.Users;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Service.Handlers.Users;
using ATMS.Admin.Service.Resources;
using ATMS.Application.Exceptions.Auth;
using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Exceptions.Enums;
using ATMS.Contracts.Events.Users;
using ATMS.Data.Enums;
using ATMS.Messaging.Configuration;
using Moq;

namespace Admin.Services.Tests.Handlers.Users;

public class UpdateUserStatusHandlerTest : BaseHandlerTest
{
    private readonly UpdateUserStatusHandler _handler;

    public UpdateUserStatusHandlerTest()
    {
        UserRepositoryMock
            .Setup(r => r.GetRolesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _handler = new UpdateUserStatusHandler(
            CurrentUserMock.Object,
            UserRepositoryMock.Object,
            UserSessionRepositoryMock.Object,
            OutboxRepositoryMock.Object,
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

    // Deactivating someone must not leave the timed lock in place.
    [Fact]
    public async Task Handle_ClearsTimedLockout()
    {
        var user = CreateUser();
        user.LockoutEnd = DateTime.UtcNow.AddMinutes(10);
        user.FailedLoginCount = 3;
        UserRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await _handler.Handle(new UpdateUserStatusCommand { Id = user.Id, UserStatusId = (int)UserStatusEnum.Inactive }, CancellationToken.None);

        Assert.Null(user.LockoutEnd);
        Assert.Equal((uint)0, user.FailedLoginCount);
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

    [Fact]
    public async Task Handle_WhenUserBecomesInactive_RevokesAllSessions()
    {
        var user = CreateUser();
        var command = new UpdateUserStatusCommand
        {
            Id = user.Id,
            UserStatusId = 2
        };

        UserRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await _handler.Handle(command, CancellationToken.None);

        UserSessionRepositoryMock.Verify(repository => repository.RevokeAllAsync(
            user.Id,
            It.IsAny<DateTime>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserBecomesActive_DoesNotRevokeSessions()
    {
        var user = CreateUser();
        user.UserStatusId = (int)UserStatusEnum.Inactive;
        UserRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await _handler.Handle(
            new UpdateUserStatusCommand { Id = user.Id, UserStatusId = (int)UserStatusEnum.Active },
            CancellationToken.None);

        UserSessionRepositoryMock.Verify(repository => repository.RevokeAllAsync(
            It.IsAny<Guid>(),
            It.IsAny<DateTime>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_PublishesWhetherTheUserIsActive()
    {
        var user = CreateUser();
        UserRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await _handler.Handle(
            new UpdateUserStatusCommand { Id = user.Id, UserStatusId = (int)UserStatusEnum.Inactive },
            CancellationToken.None);

        OutboxRepositoryMock.Verify(repository => repository.AddAsync(
            MessagingConstants.Exchanges.UserEvents,
            MessagingConstants.RoutingKeys.UserStatusChanged,
            It.Is<UserStatusChangedEvent>(message => message.Id == user.Id && !message.IsActive),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenChangingOwnStatus_ThrowsForbidden()
    {
        var user = CreateUser();
        CurrentUserMock.SetupGet(current => current.Id).Returns(user.Id);

        var exception = await Assert.ThrowsAsync<AuthException>(() =>
            _handler.Handle(
                new UpdateUserStatusCommand { Id = user.Id, UserStatusId = (int)UserStatusEnum.Inactive },
                CancellationToken.None));

        Assert.Equal(AuthErrorTypeEnum.Forbidden, exception.AuthErrorType);
        Assert.Equal(AccountMessages.CannotChangeOwnStatus, exception.Message);
        UserRepositoryMock.Verify(repository => repository.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserIsSuperAdmin_ThrowsForbidden()
    {
        var user = CreateUser();
        UserRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        UserRepositoryMock
            .Setup(r => r.GetRolesAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new Role { UserType = (int)UserTypeEnum.SuperAdmin }]);

        var exception = await Assert.ThrowsAsync<AuthException>(() =>
            _handler.Handle(
                new UpdateUserStatusCommand { Id = user.Id, UserStatusId = (int)UserStatusEnum.Inactive },
                CancellationToken.None));

        Assert.Equal(AuthErrorTypeEnum.Forbidden, exception.AuthErrorType);
        Assert.Equal(AccountMessages.CannotChangeSuperAdminStatus, exception.Message);
        OutboxRepositoryMock.Verify(repository => repository.AddAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<UserStatusChangedEvent>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenStatusIsTheSame_ChangesNothing()
    {
        var user = CreateUser();
        user.UserStatusId = (int)UserStatusEnum.Inactive;
        UserRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await _handler.Handle(
            new UpdateUserStatusCommand { Id = user.Id, UserStatusId = (int)UserStatusEnum.Inactive },
            CancellationToken.None);

        UserRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
        UserSessionRepositoryMock.Verify(r => r.RevokeAllAsync(
            It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
        OutboxRepositoryMock.Verify(r => r.AddAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<UserStatusChangedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

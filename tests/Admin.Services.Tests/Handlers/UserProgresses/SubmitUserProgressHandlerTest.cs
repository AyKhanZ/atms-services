using System.Linq.Expressions;
using ATMS.Admin.Contracts.Commands.UserProgresses;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Entities.UserProgresses;
using ATMS.Admin.Service.Handlers.UserProgresses;
using ATMS.Application.Exceptions.Auth;
using ATMS.Contracts.Events.Users;
using Moq;

namespace Admin.Services.Tests.Handlers.UserProgresses;

public class SubmitUserProgressHandlerTest : BaseHandlerTest
{
    private readonly SubmitUserProgressHandler _handler;

    public SubmitUserProgressHandlerTest()
    {
        _handler = new SubmitUserProgressHandler(
            CurrentUserMock.Object,
            UserRepositoryMock.Object,
            UserProgressRepositoryMock.Object,
            MessagePublisherMock.Object);
    }

    #region Exceptions

    [Fact]
    public async Task Handle_WhenProgressNotFound_ThrowsAuthException()
    {
        // Arrange
        CurrentUserMock.Setup(x => x.Id).Returns(Guid.NewGuid());

        UserProgressRepositoryMock
            .Setup(x => x.FindAsync(
                It.IsAny<Expression<Func<UserProgress, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserProgress?)null);

        var command = new SubmitUserProgressCommand();

        // Act & Assert
        await Assert.ThrowsAsync<AuthException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ThrowsAuthException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        CurrentUserMock.Setup(x => x.Id).Returns(userId);

        UserProgressRepositoryMock
            .Setup(x => x.FindAsync(
                It.IsAny<Expression<Func<UserProgress, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserProgress { UserId = userId, PersonalInfo = new PersonalInfo() });

        UserRepositoryMock
            .Setup(x => x.FindAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new SubmitUserProgressCommand();

        // Act & Assert
        await Assert.ThrowsAsync<AuthException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    #endregion

    #region Happy path

    [Fact]
    public async Task Handle_WhenValid_SubmitsAndPublishesEvents()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();

        var progress = new UserProgress
        {
            UserId = userId,
            OrganizationId = organizationId,
            PersonalInfo = new PersonalInfo
            {
                Name = "John",
                Surname = "Doe",
                AvatarPath = "avatar.png"
            },
            InvitedUsers =
            [
                new InvitedUser { Email = "a@test.com", Name = "A", Surname = "A" },
                new InvitedUser { Email = "b@test.com", Name = "B", Surname = "B" }
            ]
        };

        var user = new User
        {
            Id = userId
        };

        CurrentUserMock.Setup(x => x.Id).Returns(userId);

        UserProgressRepositoryMock
            .Setup(x => x.FindAsync(
                It.IsAny<Expression<Func<UserProgress, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(progress);

        UserRepositoryMock
            .Setup(x => x.FindAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new SubmitUserProgressCommand();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert

        // Submit
        UserProgressRepositoryMock.Verify(x =>
            x.SubmitAsync(progress, user, It.IsAny<CancellationToken>()), Times.Once);

        // UserUpdatedEvent
        MessagePublisherMock.Verify(x =>
            x.PublishAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.Is<UserUpdatedEvent>(e =>
                    e.Id == userId &&
                    e.Name == "John" &&
                    e.Surname == "Doe"),
                It.IsAny<CancellationToken>()),
            Times.Once);

        // UserInvitedEvent (2 users → 2 events)
        MessagePublisherMock.Verify(x =>
            x.PublishAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<UserInvitedEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    #endregion

    #region No invited users

    [Fact]
    public async Task Handle_WhenNoInvitedUsers_PublishesOnlyUserUpdatedEvent()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var progress = new UserProgress
        {
            UserId = userId,
            PersonalInfo = new PersonalInfo
            {
                Name = "John",
                Surname = "Doe"
            },
            InvitedUsers = null
        };

        var user = new User { Id = userId };

        CurrentUserMock.Setup(x => x.Id).Returns(userId);

        UserProgressRepositoryMock
            .Setup(x => x.FindAsync(
                It.IsAny<Expression<Func<UserProgress, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(progress);

        UserRepositoryMock
            .Setup(x => x.FindAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new SubmitUserProgressCommand();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        MessagePublisherMock.Verify(x =>
            x.PublishAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<UserUpdatedEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        MessagePublisherMock.Verify(x =>
            x.PublishAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<UserInvitedEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion
}
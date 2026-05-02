using System.Linq.Expressions;
using ATMS.Admin.Contracts.Commands.Profile;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Service.Handlers.Profile;
using ATMS.Application.Exceptions.Entity;
using Moq;

namespace Admin.Services.Tests.Handlers.Profile;

public class UpdatePhotoHandlerTest : BaseHandlerTest
{
    private readonly UpdatePhotoHandler _handler;

    public UpdatePhotoHandlerTest()
    {
        _handler = new UpdatePhotoHandler(
            UserRepositoryMock.Object,
            MessagePublisherMock.Object,
            CacheServiceMock.Object);
    }

    private User CreateUser() => new() { Id = Guid.NewGuid(), AvatarPath = "old-photo.jpg" };

    [Fact]
    public async Task Handle_UpdatesAvatarPath()
    {
        // Arrange
        var user = CreateUser();
        var command = new UpdatePhotoCommand { Id = user.Id, FileName = "new-photo.jpg" };

        UserRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(command.FileName, user.AvatarPath);
    }

    [Fact]
    public async Task Handle_SavesChanges()
    {
        // Arrange
        var user = CreateUser();
        var command = new UpdatePhotoCommand { Id = user.Id, FileName = "new-photo.jpg" };

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
        var command = new UpdatePhotoCommand { Id = Guid.NewGuid(), FileName = "new-photo.jpg" };

        UserRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<EntityException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }
}
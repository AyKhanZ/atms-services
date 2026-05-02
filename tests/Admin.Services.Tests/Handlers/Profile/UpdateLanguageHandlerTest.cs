using System.Linq.Expressions;
using ATMS.Admin.Contracts.Commands.Profile;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Service.Handlers.Profile;
using ATMS.Application.Exceptions.Entity;
using Moq;

namespace Admin.Services.Tests.Handlers.Profile;

public class UpdateLanguageHandlerTest : BaseHandlerTest
{
    private readonly UpdateLanguageHandler _handler;

    public UpdateLanguageHandlerTest()
    {
        _handler = new UpdateLanguageHandler(
            UserRepositoryMock.Object,
            CacheServiceMock.Object);
    }

    private User CreateUser() => new User { Id = Guid.NewGuid(), Language = "en" };

    [Fact]
    public async Task Handle_UpdatesLanguage()
    {
        // Arrange
        var user = CreateUser();
        var command = new UpdateLanguageCommand { Id = user.Id, Language = "az" };

        UserRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(command.Language, user.Language);
    }

    [Fact]
    public async Task Handle_SavesChanges()
    {
        // Arrange
        var user = CreateUser();
        var command = new UpdateLanguageCommand { Id = user.Id, Language = "az" };

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
        var command = new UpdateLanguageCommand { Id = Guid.NewGuid(), Language = "az" };

        UserRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<EntityException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }
}
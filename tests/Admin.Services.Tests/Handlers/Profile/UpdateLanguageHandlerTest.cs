using System.Linq.Expressions;
using ATMS.Admin.Contracts.Commands.Profile;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Entities.Dictionaries;
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
            DictionariesRepositoryMock.Object,
            CacheServiceMock.Object);

        DictionariesRepositoryMock
            .Setup(x => x.GetLanguagesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new Language { Id = 1, Code = "AZ", Name = "Azerbaijani", NativeName = "Azərbaycanca" },
                new Language { Id = 2, Code = "EN", Name = "English", NativeName = "English" }
            ]);
    }

    private User CreateUser() => new User { Id = Guid.NewGuid(), LanguageId = 2 };

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
        Assert.Equal(1, user.LanguageId);
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

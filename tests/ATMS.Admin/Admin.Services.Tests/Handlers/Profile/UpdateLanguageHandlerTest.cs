using System.Linq.Expressions;
using ATMS.Admin.Contracts.Commands.Profile;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Entities.Dictionaries;
using ATMS.Admin.Service.Handlers.Profile;
using ATMS.Application.Exceptions.Entity;
using ATMS.Caching.Constants;
using ATMS.Contracts.Events.Users;
using ATMS.Messaging.Configuration;
using Moq;

namespace Admin.Services.Tests.Handlers.Profile;

public class UpdateLanguageHandlerTest : BaseHandlerTest
{
    private readonly UpdateLanguageHandler _handler;

    public UpdateLanguageHandlerTest()
    {
        _handler = new UpdateLanguageHandler(
            CurrentUserMock.Object,
            UserRepositoryMock.Object,
            DictionariesRepositoryMock.Object,
            OutboxRepositoryMock.Object,
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
        CurrentUserMock.SetupGet(x => x.Id).Returns(user.Id);
        var command = new UpdateLanguageCommand { Language = "az" };

        UserRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(1, user.LanguageId);
        CacheServiceMock.Verify(x => x.RemoveAsync(CacheKeys.Admin.ProfileById(user.Id), It.IsAny<CancellationToken>()), Times.Once);
        OutboxRepositoryMock.Verify(x => x.AddAsync(
            MessagingConstants.Exchanges.UserEvents,
            MessagingConstants.RoutingKeys.UserUpdated,
            It.Is<UserUpdatedEvent>(message => message.Id == user.Id && message.Language == "az"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_SavesChanges()
    {
        // Arrange
        var user = CreateUser();
        CurrentUserMock.SetupGet(x => x.Id).Returns(user.Id);
        var command = new UpdateLanguageCommand { Language = "az" };

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
        var command = new UpdateLanguageCommand { Language = "az" };

        UserRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<EntityException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }
}

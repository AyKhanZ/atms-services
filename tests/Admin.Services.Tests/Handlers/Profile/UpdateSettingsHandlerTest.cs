using System.Linq.Expressions;
using ATMS.Admin.Contracts.Commands.Profile;
using ATMS.Admin.Contracts.Models.Profile;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Service.Handlers.Profile;
using ATMS.Contracts.Events.Users;
using ATMS.Infrastructure.Images;
using ATMS.Caching.Constants;
using ATMS.Application.Exceptions.Entity;
using ATMS.Messaging.Configuration;
using ATMS.Data.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace Admin.Services.Tests.Handlers.Profile;

public class UpdateSettingsHandlerTest : BaseHandlerTest
{
    private readonly Mock<IImageStorage> _imageStorage = new();
    private readonly Mock<ILogger<UpdateSettingsHandler>> _logger = new();

    private UpdateSettingsHandler CreateHandler()
    {
        MapperMock.Setup(x => x.Map<ProfileModel>(It.IsAny<User>()))
            .Returns((User user) => new ProfileModel
            {
                Email = user.Email,
                AvatarPath = user.AvatarPath,
                BirthDate = user.BirthDate.HasValue ? DateOnly.FromDateTime(user.BirthDate.Value) : null
            });

        return new UpdateSettingsHandler(
            CurrentUserMock.Object,
            UserRepositoryMock.Object,
            OutboxRepositoryMock.Object,
            _imageStorage.Object,
            CacheServiceMock.Object,
            MapperMock.Object,
            _logger.Object);
    }

    private static UpdateSettingsCommand Command(IFormFile? avatar = null) => new()
    {
        Name = "New name",
        Surname = "New surname",
        PhoneNumber = "+994501234567",
        Position = "Developer",
        BirthDate = new DateOnly(1990, 1, 2),
        GenderId = 1,
        MaritalStatusId = 1,
        LanguageId = 2,
        Avatar = avatar
    };

    [Fact]
    public async Task Handle_SavesCurrentUsersProfileAndQueuesEvent()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "user@example.com", AvatarPath = "old.webp" };
        CurrentUserMock.SetupGet(x => x.Id).Returns(user.Id);
        UserRepositoryMock.Setup(x => x.FindAsync(
                It.Is<Expression<Func<User, bool>>>(predicate =>
                    predicate.Compile()(user) && !predicate.Compile()(new User { Id = Guid.NewGuid() })),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await CreateHandler().Handle(Command(), CancellationToken.None);

        Assert.Equal(user.Id, CurrentUserMock.Object.Id);
        Assert.Equal(2, user.LanguageId);
        Assert.Equal(new DateTime(1990, 1, 2), user.BirthDate);
        // Npgsql refuses a non-UTC DateTime for timestamptz; DateTime equality above ignores Kind.
        Assert.Equal(DateTimeKind.Utc, user.BirthDate!.Value.Kind);
        Assert.Equal("user@example.com", result.Email);
        Assert.Equal(new DateOnly(1990, 1, 2), result.BirthDate);
        OutboxRepositoryMock.Verify(x => x.AddAsync(
            MessagingConstants.Exchanges.UserEvents,
            MessagingConstants.RoutingKeys.UserUpdated,
            It.Is<UserUpdatedEvent>(e => e.Id == user.Id),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_SaveFails_DeletesNewAvatar()
    {
        var user = new User { Id = Guid.NewGuid(), AvatarPath = "old.webp" };
        var avatar = new Mock<IFormFile>().Object;
        CurrentUserMock.SetupGet(x => x.Id).Returns(user.Id);
        UserRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _imageStorage.Setup(x => x.SaveAsync(avatar, ImageStorageFolder.Users, user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StoredImage("new.webp", "url", "image/webp", 10));
        UserRepositoryMock.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException());

        await Assert.ThrowsAsync<InvalidOperationException>(() => CreateHandler().Handle(Command(avatar), CancellationToken.None));

        _imageStorage.Verify(x => x.DeleteAsync("new.webp", CancellationToken.None), Times.Once);
        _imageStorage.Verify(x => x.DeleteAsync("old.webp", It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReplacementAvatar_DeletesOldFileAfterSaveAndInvalidatesProfile()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "user@example.com", AvatarPath = "old.webp" };
        var avatar = new Mock<IFormFile>().Object;
        CurrentUserMock.SetupGet(x => x.Id).Returns(user.Id);
        UserRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _imageStorage.Setup(x => x.SaveAsync(avatar, ImageStorageFolder.Users, user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StoredImage("new.webp", "url", "image/webp", 10));

        var result = await CreateHandler().Handle(Command(avatar), CancellationToken.None);

        Assert.Equal("new.webp", result.AvatarPath);
        _imageStorage.Verify(x => x.DeleteAsync("old.webp", It.IsAny<CancellationToken>()), Times.Once);
        CacheServiceMock.Verify(x => x.RemoveAsync(CacheKeys.Admin.ProfileById(user.Id), It.IsAny<CancellationToken>()), Times.Once);
        CacheServiceMock.Verify(x => x.RemoveAsync(CacheKeys.Admin.MeById(user.Id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CurrentUserNotFound_ThrowsNotFoundWithoutSaving()
    {
        UserRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<EntityException>(() => CreateHandler().Handle(Command(), CancellationToken.None));

        UserRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(DefaultValues.UserAvatar)]
    [InlineData("")]
    public async Task Handle_ReplacementAvatar_DoesNotDeleteSharedOrEmptyAvatar(string oldAvatarPath)
    {
        var user = new User { Id = Guid.NewGuid(), AvatarPath = oldAvatarPath };
        var avatar = new Mock<IFormFile>().Object;
        CurrentUserMock.SetupGet(x => x.Id).Returns(user.Id);
        UserRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _imageStorage.Setup(x => x.SaveAsync(avatar, ImageStorageFolder.Users, user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StoredImage("new.webp", "url", "image/webp", 10));

        await CreateHandler().Handle(Command(avatar), CancellationToken.None);

        _imageStorage.Verify(x => x.DeleteAsync(oldAvatarPath, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_OldAvatarDeletionFails_ReturnsSavedProfileAfterCacheInvalidation()
    {
        var user = new User { Id = Guid.NewGuid(), AvatarPath = "old.webp" };
        var avatar = new Mock<IFormFile>().Object;
        var operations = new List<string>();
        CurrentUserMock.SetupGet(x => x.Id).Returns(user.Id);
        UserRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _imageStorage.Setup(x => x.SaveAsync(avatar, ImageStorageFolder.Users, user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StoredImage("new.webp", "url", "image/webp", 10));
        CacheServiceMock.Setup(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback(() => operations.Add("cache"))
            .Returns(Task.CompletedTask);
        _imageStorage.Setup(x => x.DeleteAsync("old.webp", It.IsAny<CancellationToken>()))
            .Callback(() => operations.Add("delete"))
            .ThrowsAsync(new IOException("Deletion failed"));

        var result = await CreateHandler().Handle(Command(avatar), CancellationToken.None);

        Assert.Equal("new.webp", result.AvatarPath);
        Assert.Equal("delete", operations.Last());
        Assert.All(operations.SkipLast(1), operation => Assert.Equal("cache", operation));
        _logger.Verify(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((_, _) => true),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }
}

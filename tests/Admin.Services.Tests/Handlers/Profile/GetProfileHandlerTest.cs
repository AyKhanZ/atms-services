using ATMS.Admin.Contracts.Models.Profile;
using ATMS.Admin.Contracts.Requests.Profile;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Service.Handlers.Profile;
using ATMS.Caching.Constants;
using Moq;

namespace Admin.Services.Tests.Handlers.Profile;

public class GetProfileHandlerTest : BaseHandlerTest
{
    [Fact]
    public async Task Handle_CacheHit_ReturnsCachedProfileWithoutDatabaseRead()
    {
        var model = new ProfileModel
        {
            Name = "Jane", Surname = "Doe", Email = "jane@example.com", PhoneNumber = "+994501234567",
            Position = "Developer", AvatarPath = "avatar.webp", LanguageId = 1,
            BirthDate = new DateOnly(1990, 1, 1), GenderId = 1, MaritalStatusId = 1
        };
        CacheServiceMock.Setup(x => x.GetOrSetAsync(
                CacheKeys.Admin.ProfileById(CurrentUserMock.Object.Id),
                It.IsAny<Func<Task<ProfileModel>>>(), CacheTtl.Entity, It.IsAny<CancellationToken>()))
            .ReturnsAsync(model);

        var result = await new GetProfileHandler(CurrentUserMock.Object, UserRepositoryMock.Object, CacheServiceMock.Object, MapperMock.Object)
            .Handle(new GetProfileRequest(), CancellationToken.None);

        Assert.Same(model, result);
        UserRepositoryMock.Verify(x => x.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CacheMiss_ReadsCurrentUserAndUsesConfiguredTtl()
    {
        var userId = CurrentUserMock.Object.Id;
        UserRepositoryMock.Setup(x => x.GetAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User
            {
                Id = userId, Name = "Jane", Surname = "Doe", Email = "jane@example.com",
                PhoneNumber = "+994501234567", Position = "Developer", AvatarPath = "avatar.webp",
                BirthDate = new DateTime(1990, 1, 1), LanguageId = 1, GenderId = 1, MaritalStatusId = 1
            });
        CacheServiceMock.Setup(x => x.GetOrSetAsync(
                CacheKeys.Admin.ProfileById(userId), It.IsAny<Func<Task<ProfileModel>>>(),
                CacheTtl.Entity, It.IsAny<CancellationToken>()))
            .Returns((string _, Func<Task<ProfileModel>> factory, TimeSpan _, CancellationToken _) => factory());
        MapperMock.Setup(x => x.Map<ProfileModel>(It.IsAny<User>()))
            .Returns(new ProfileModel { Email = "jane@example.com", BirthDate = new DateOnly(1990, 1, 1) });

        var result = await new GetProfileHandler(CurrentUserMock.Object, UserRepositoryMock.Object, CacheServiceMock.Object, MapperMock.Object)
            .Handle(new GetProfileRequest(), CancellationToken.None);

        Assert.Equal("jane@example.com", result.Email);
        Assert.Equal(new DateOnly(1990, 1, 1), result.BirthDate);
    }

    [Fact]
    public async Task Handle_PersonalInfoIncomplete_ReturnsStoredNullableFields()
    {
        var userId = CurrentUserMock.Object.Id;
        UserRepositoryMock.Setup(x => x.GetAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId, Name = "Jane", Surname = "Doe", Email = "jane@example.com" });
        CacheServiceMock.Setup(x => x.GetOrSetAsync(
                CacheKeys.Admin.ProfileById(userId), It.IsAny<Func<Task<ProfileModel>>>(),
                CacheTtl.Entity, It.IsAny<CancellationToken>()))
            .Returns((string _, Func<Task<ProfileModel>> factory, TimeSpan _, CancellationToken _) => factory());
        MapperMock.Setup(x => x.Map<ProfileModel>(It.IsAny<User>()))
            .Returns(new ProfileModel { Name = "Jane", Email = "jane@example.com" });

        var result = await new GetProfileHandler(CurrentUserMock.Object, UserRepositoryMock.Object, CacheServiceMock.Object, MapperMock.Object)
            .Handle(new GetProfileRequest(), CancellationToken.None);

        Assert.Null(result.PhoneNumber);
        Assert.Null(result.Position);
        Assert.Null(result.BirthDate);
        Assert.Null(result.GenderId);
        Assert.Null(result.MaritalStatusId);
    }
}

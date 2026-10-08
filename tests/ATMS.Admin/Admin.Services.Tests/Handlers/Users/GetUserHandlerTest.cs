using ATMS.Admin.Contracts.Models.Organizations;
using ATMS.Admin.Contracts.Models.Users;
using ATMS.Admin.Contracts.Requests.Users;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Entities.Dictionaries;
using ATMS.Admin.Service.Handlers.Users;
using ATMS.Admin.Service.Providers.Interfaces;
using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Exceptions.Enums;
using ATMS.Application.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Admin.Services.Tests.Handlers.Users;

public class GetUserHandlerTest : BaseHandlerTest
{
    private readonly Mock<IOrganizationProvider> _organizationProviderMock = new();
    private readonly GetUserHandler _handler;
    
    public GetUserHandlerTest()
    {
        _handler = new GetUserHandler(
            UserRepositoryMock.Object,
            _organizationProviderMock.Object,
            MapperMock.Object,
            CacheServiceMock.Object,
            NullLogger<GetUserHandler>.Instance);
    }
    
    private User CreateUser(Guid? id = null) =>
        new()
        {
            Id = id ?? Guid.NewGuid(),
            Email = Faker.Internet.Email(),
            Name = Faker.Name.FirstName(),
            Surname = Faker.Name.LastName(),
            Gender = new Gender
            {
                Translations = new List<GenderTranslation>()
            },
            MaritalStatus = new MaritalStatus
            {
                Translations = new List<MaritalStatusTranslation>()
            },
            UserStatus = new UserStatus
            {
                Translations = new List<UserStatusTranslation>()
            },
            UserRoles = []
        };
    

    [Fact]
    public async Task Handle_WhenUserExists_ReturnsMappedModel()
    {
        var user = CreateUser();
        var expectedModel = new UserModel { Id = user.Id };
        var request = new GetUserRequest { Id = user.Id };

        CacheServiceMock
            .Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<UserModel>>>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<Task<UserModel>>, TimeSpan, CancellationToken>(
                (_, factory, _, _) => factory()!);
        
        UserRepositoryMock
            .Setup(r => r.GetAsync(
                user.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
 
        MapperMock
            .Setup(m => m.Map<UserModel>(user))
            .Returns(expectedModel);
 
        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.Equal(expectedModel, result);

    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ThrowsEntityException()
    {
        var request = new GetUserRequest { Id = Guid.NewGuid() };

        UserRepositoryMock
            .Setup(r => r.GetAsync(request.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
 
        var exception = await Assert.ThrowsAsync<EntityException>(() =>
            _handler.Handle(request, CancellationToken.None));
 
        Assert.Equal(EntityErrorTypeEnum.NotFound, exception.ErrorType);
    }
    
    [Fact]
    public async Task Handle_Should_Map_UserRoles()
    {
        // Arrange
        var user = CreateUser();
        var role = new Role { Id = Guid.NewGuid(), Name = "Admin" };

        user.UserRoles = [new UserRole { Role = role }];

        var request = new GetUserRequest { Id = user.Id };

        CacheServiceMock
            .Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<UserModel>>>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<Task<UserModel>>, TimeSpan, CancellationToken>(
                (_, factory, _, _) => factory()!);
        
        UserRepositoryMock
            .Setup(r => r.GetAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        MapperMock
            .Setup(m => m.Map<UserModel>(user))
            .Returns(new UserModel());

        MapperMock
            .Setup(m => m.Map<DictionaryModel<Guid>>(role))
            .Returns(new DictionaryModel<Guid> { Id = role.Id });

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.Single(result.Roles);
    }
    
    [Fact]
    public async Task Handle_WhenNoRoles_ReturnsEmptyRoles()
    {
        // Arrange
        var user = CreateUser();
        user.UserRoles = [];

        var request = new GetUserRequest { Id = user.Id };

        CacheServiceMock
            .Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<UserModel>>>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<Task<UserModel>>, TimeSpan, CancellationToken>(
                (_, factory, _, _) => factory()!);
        
        UserRepositoryMock
            .Setup(r => r.GetAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        MapperMock
            .Setup(m => m.Map<UserModel>(user))
            .Returns(new UserModel());

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.Empty(result.Roles);
    }
    
    [Fact]
    public async Task Handle_Should_Call_GetAsync_With_RequestId()
    {
        // Arrange
        var user = CreateUser();
        var request = new GetUserRequest { Id = user.Id };
        
        CacheServiceMock
            .Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<UserModel>>>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<Task<UserModel>>, TimeSpan, CancellationToken>(
                (_, factory, _, _) => factory()!);
        
        UserRepositoryMock
            .Setup(r => r.GetAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        MapperMock
            .Setup(m => m.Map<UserModel>(user))
            .Returns(new UserModel());

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        UserRepositoryMock.Verify(r =>
                r.GetAsync(user.Id, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserHasNoOrganization_DoesNotAskProjectService()
    {
        // Arrange
        var user = CreateUser();
        user.OrganizationId = null;
        SetupCachePassThrough();
        SetupUser(user);

        // Act
        var result = await _handler.Handle(new GetUserRequest { Id = user.Id }, CancellationToken.None);

        // Assert
        Assert.Null(result.Organization);
        _organizationProviderMock.Verify(
            p => p.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserBelongsToOrganization_FillsOrganization()
    {
        // Arrange
        var user = CreateUser();
        user.OrganizationId = Guid.NewGuid();
        var organization = new OrganizationModel { Id = user.OrganizationId.Value, Title = "Apple", Voen = "8056783562" };
        SetupCachePassThrough();
        SetupUser(user);

        _organizationProviderMock
            .Setup(p => p.GetAsync(user.OrganizationId.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(organization);

        // Act
        var result = await _handler.Handle(new GetUserRequest { Id = user.Id }, CancellationToken.None);

        // Assert
        Assert.Same(organization, result.Organization);
    }

    // Project is down or slow: the user card still opens, only without the organization row.
    [Theory]
    [InlineData(typeof(HttpRequestException))]
    [InlineData(typeof(TaskCanceledException))]
    public async Task Handle_WhenProjectServiceFails_ReturnsUserWithoutOrganization(Type exceptionType)
    {
        // Arrange
        var user = CreateUser();
        user.OrganizationId = Guid.NewGuid();
        SetupCachePassThrough();
        SetupUser(user);

        _organizationProviderMock
            .Setup(p => p.GetAsync(user.OrganizationId.Value, It.IsAny<CancellationToken>()))
            .ThrowsAsync((Exception)Activator.CreateInstance(exceptionType)!);

        // Act
        var result = await _handler.Handle(new GetUserRequest { Id = user.Id }, CancellationToken.None);

        // Assert
        Assert.Equal(user.Id, result.Id);
        Assert.Null(result.Organization);
    }

    // The organization is cached with the user: a title or logo change shows within the cache lifetime.
    [Fact]
    public async Task Handle_WhenUserComesFromCache_DoesNotAskProjectService()
    {
        // Arrange
        var organization = new OrganizationModel { Id = Guid.NewGuid(), Title = "Apple", Voen = "8056783562" };
        var cached = new UserModel { Id = Guid.NewGuid(), Organization = organization };
        CacheServiceMock
            .Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<UserModel>>>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cached);

        // Act
        var result = await _handler.Handle(new GetUserRequest { Id = cached.Id }, CancellationToken.None);

        // Assert
        Assert.Same(organization, result.Organization);
        _organizationProviderMock.Verify(
            p => p.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // The organization is deleted in Project while Admin still holds its id: the card opens without it.
    [Fact]
    public async Task Handle_WhenOrganizationNoLongerExists_LeavesOrganizationEmpty()
    {
        // Arrange
        var user = CreateUser();
        user.OrganizationId = Guid.NewGuid();
        SetupCachePassThrough();
        SetupUser(user);

        _organizationProviderMock
            .Setup(p => p.GetAsync(user.OrganizationId.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrganizationModel?)null);

        // Act
        var result = await _handler.Handle(new GetUserRequest { Id = user.Id }, CancellationToken.None);

        // Assert
        Assert.Null(result.Organization);
    }

    private void SetupCachePassThrough()
    {
        CacheServiceMock
            .Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<UserModel>>>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<Task<UserModel>>, TimeSpan, CancellationToken>(
                (_, factory, _, _) => factory()!);
    }

    private void SetupUser(User user)
    {
        UserRepositoryMock
            .Setup(r => r.GetAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        MapperMock
            .Setup(m => m.Map<UserModel>(user))
            .Returns(new UserModel { Id = user.Id });
    }
}
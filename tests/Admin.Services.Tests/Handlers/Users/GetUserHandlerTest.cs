using ATMS.Admin.Contracts.Models.Users;
using ATMS.Admin.Contracts.Requests.Users;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Entities.Dictionaries;
using ATMS.Admin.Service.Handlers.Users;
using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Models;
using Moq;

namespace Admin.Services.Tests.Handlers.Users;

public class GetUserHandlerTest : BaseHandlerTest
{
    private readonly GetUserHandler _handler;
    
    public GetUserHandlerTest()
    {
        _handler = new GetUserHandler(UserRepositoryMock.Object, MapperMock.Object);
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
 
        Assert.Equal(EntityErrorType.NotFound, exception.ErrorType);
    }
    
    [Fact]
    public async Task Handle_Should_Map_UserRoles()
    {
        // Arrange
        var user = CreateUser();
        var role = new Role { Id = Guid.NewGuid(), Name = "Admin" };

        user.UserRoles = [new UserRole { Role = role }];

        var request = new GetUserRequest { Id = user.Id };

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
}

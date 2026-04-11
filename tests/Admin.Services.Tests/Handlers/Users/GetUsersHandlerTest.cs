using ATMS.Admin.Contracts.Models.Users;
using ATMS.Admin.Contracts.Requests.Users;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Entities.Dictionaries;
using ATMS.Admin.Service.Handlers.Users;
using Moq;

namespace Admin.Services.Tests.Handlers.Users;

public class GetUsersHandlerTest : BaseHandlerTest
{
    private readonly GetUsersHandler _handler;
 
    public GetUsersHandlerTest()
    {
        _handler = new GetUsersHandler(UserRepositoryMock.Object, MapperMock.Object);
    }

    private User CreateUser()
    {
        return new User
        {
            UserStatus = new UserStatus
            {
                Translations = new List<UserStatusTranslation>()
            }
        };
    }

    [Fact]
    public async Task Handle_ReturnsMappedUsers()
    {
        // Arrange
        var users = new List<User>
        {
            CreateUser(),
            CreateUser()
        };

        UserRepositoryMock
            .Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        MapperMock
            .Setup(m => m.Map<UserListItemModel>(It.IsAny<User>()))
            .Returns(new UserListItemModel());

        // Act
        var result = await _handler.Handle(new GetUsersRequest(), CancellationToken.None);

        // Assert
        Assert.Equal(users.Count, result.Length);
    }
 
    [Fact]
    public async Task Handle_WhenNoUsers_ReturnsEmptyArray()
    {
        // Arrange
        UserRepositoryMock
            .Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var result = await _handler.Handle(new GetUsersRequest(), CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }
    
    [Fact]
    public async Task Handle_Should_Map_Each_User()
    {
        // Arrange
        var users = new List<User>
        {
            CreateUser(),
            CreateUser(),
            CreateUser()
        };

        UserRepositoryMock
            .Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        MapperMock
            .Setup(m => m.Map<UserListItemModel>(It.IsAny<User>()))
            .Returns(new UserListItemModel());

        // Act
        await _handler.Handle(new GetUsersRequest(), CancellationToken.None);

        // Assert
        MapperMock.Verify(m =>
                m.Map<UserListItemModel>(It.IsAny<User>()),
            Times.Exactly(users.Count));
    }
    
    [Fact]
    public async Task Handle_WhenSingleUser_ReturnsSingleModel()
    {
        // Arrange
        var users = new List<User> { CreateUser() };

        UserRepositoryMock
            .Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        MapperMock
            .Setup(m => m.Map<UserListItemModel>(It.IsAny<User>()))
            .Returns(new UserListItemModel());

        // Act
        var result = await _handler.Handle(new GetUsersRequest(), CancellationToken.None);

        // Assert
        Assert.Single(result);
    }
}

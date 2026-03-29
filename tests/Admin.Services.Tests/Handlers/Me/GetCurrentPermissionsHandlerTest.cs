using System.Linq.Expressions;
using ATMS.Admin.Contracts.Requests.Me;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Entities.Dictionaries;
using ATMS.Admin.Service.Handlers.Me;
using ATMS.Application.Exceptions.Auth;
using Moq;

namespace Admin.Services.Tests.Handlers.Me;

public class GetCurrentPermissionsHandlerTest : BaseHandlerTest
{
    private readonly GetCurrentPermissionsHandler _handler;

    public GetCurrentPermissionsHandlerTest()
    {
        _handler = new GetCurrentPermissionsHandler(UserRepositoryMock.Object, СurrentUserMock.Object);
    }
 
    [Fact]
    public async Task Handle_WhenUserExists_ReturnsPermissionCodes()
    {
        string[] expected = ["users.read", "users.write"];
        var request = new GetCurrentPermissionsRequest();
        var userId = Guid.NewGuid();

        СurrentUserMock
            .Setup(c => c.Id)
            .Returns(userId);
        var permissions = new List<Permission>
        {
            new() { Code = "users.read" },
            new() { Code = "users.write" }
        };
 
        UserRepositoryMock
            .Setup(r => r.IsExistAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
 
        UserRepositoryMock
            .Setup(r => r.GetPermissionsAsync(userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);
 
        var result = await _handler.Handle(request, CancellationToken.None);
 
        Assert.Equal(expected, result);
    }
 
    [Fact]
    public async Task Handle_WhenUserNotFound_ThrowsEntityException()
    {
        var request = new GetCurrentPermissionsRequest();
 
        UserRepositoryMock
            .Setup(r => r.IsExistAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
 
        var exception = await Assert.ThrowsAsync<AuthException>(() =>
            _handler.Handle(request, CancellationToken.None));

        Assert.Equal(AuthErrorType.InvalidCredentials, exception.AuthErrorType);
    }
    
    [Fact]
    public async Task Handle_Should_Use_CurrentUser_Id_For_GetPermissions()
    {
        // Arrange
        var request = new GetCurrentPermissionsRequest();
        var userId = Guid.NewGuid();

        СurrentUserMock
            .Setup(c => c.Id)
            .Returns(userId);

        UserRepositoryMock
            .Setup(r => r.IsExistAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        UserRepositoryMock
            .Setup(r => r.GetPermissionsAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        UserRepositoryMock.Verify(r =>
                r.GetPermissionsAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }
    
    [Fact]
    public async Task Handle_WhenNoPermissions_ReturnsEmptyArray()
    {
        // Arrange
        var request = new GetCurrentPermissionsRequest();
        var userId = Guid.NewGuid();

        СurrentUserMock
            .Setup(c => c.Id)
            .Returns(userId);

        UserRepositoryMock
            .Setup(r => r.IsExistAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        UserRepositoryMock
            .Setup(r => r.GetPermissionsAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}

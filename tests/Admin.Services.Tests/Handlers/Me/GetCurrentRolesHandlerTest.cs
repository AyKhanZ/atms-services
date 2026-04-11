using System.Linq.Expressions;
using ATMS.Admin.Contracts.Requests.Me;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Service.Handlers.Me;
using ATMS.Application.Exceptions.Auth;
using ATMS.Application.Models;
using Moq;

namespace Admin.Services.Tests.Handlers.Me;

public class GetCurrentRolesHandlerTest : BaseHandlerTest
{
    private readonly GetCurrentRolesHandler _handler;
 
    public GetCurrentRolesHandlerTest()
    {
        _handler = new GetCurrentRolesHandler(UserRepositoryMock.Object, СurrentUserMock.Object, MapperMock.Object);
    }
 
    [Fact]
    public async Task Handle_WhenUserExists_ReturnsMappedRoles()
    {
        var request = new GetCurrentRolesRequest();
        var roles = new List<Role> { new() { Id = Guid.NewGuid(), Name = "Admin" } };
        var expectedModels = new[] { new DictionaryModel<Guid> { Id = roles[0].Id, Name = roles[0].Name } };
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
            .Setup(r => r.GetRolesAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);
 
        MapperMock
            .Setup(m => m.Map<DictionaryModel<Guid>[]>(roles))
            .Returns(expectedModels);
 
        var result = await _handler.Handle(request, CancellationToken.None);
 
        Assert.Equal(expectedModels, result);
    }
 
    [Fact]
    public async Task Handle_WhenUserNotFound_ThrowsEntityException()
    {
        var request = new GetCurrentRolesRequest();
        UserRepositoryMock
            .Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
 
        var exception = await Assert.ThrowsAsync<AuthException>(() =>
            _handler.Handle(request, CancellationToken.None));

        Assert.Equal(AuthErrorType.InvalidCredentials, exception.AuthErrorType);
    }
    
    [Fact]
    public async Task Handle_Should_Use_CurrentUser_Id_For_GetRoles()
    {
        // Arrange
        var request = new GetCurrentRolesRequest();
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
            .Setup(r => r.GetRolesAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        MapperMock
            .Setup(m => m.Map<DictionaryModel<Guid>[]>(It.IsAny<object>()))
            .Returns([]);

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        UserRepositoryMock.Verify(r =>
                r.GetRolesAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }
    
    [Fact]
    public async Task Handle_WhenNoRoles_ReturnsEmptyArray()
    {
        // Arrange
        var request = new GetCurrentRolesRequest();
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
            .Setup(r => r.GetRolesAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        MapperMock
            .Setup(m => m.Map<DictionaryModel<Guid>[]>(It.IsAny<object>()))
            .Returns([]);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}

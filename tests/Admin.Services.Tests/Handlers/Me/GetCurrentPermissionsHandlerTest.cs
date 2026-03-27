using System.Linq.Expressions;
using ATMS.Admin.Contracts.Requests.Me;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Entities.Dictionaries;
using ATMS.Admin.Service.Handlers.Me;
using ATMS.Application.Exceptions.Entity;
using Moq;

namespace Admin.Services.Tests.Handlers.Me;

public class GetCurrentPermissionsHandlerTest : BaseHandlerTest
{
    private readonly GetCurrentPermissionsHandler _handler;
 
    public GetCurrentPermissionsHandlerTest()
    {
        _handler = new GetCurrentPermissionsHandler(UserRepositoryMock.Object);
    }
 
    private GetCurrentPermissionsRequest CreateRequest(Guid? userId = null) =>
        new() { UserId = userId ?? Guid.NewGuid() };
 
    [Fact]
    public async Task Handle_WhenUserExists_ReturnsPermissionCodes()
    {
        var request = CreateRequest();
        var permissions = new List<Permission>
        {
            new() { Code = "users.read" },
            new() { Code = "users.write" }
        };
 
        UserRepositoryMock
            .Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
 
        UserRepositoryMock
            .Setup(r => r.GetPermissionsAsync(request.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);
 
        var result = await _handler.Handle(request, CancellationToken.None);
 
        Assert.Equal(new[] { "users.read", "users.write" }, result);
    }
 
    [Fact]
    public async Task Handle_WhenUserNotFound_ThrowsEntityException()
    {
        var request = CreateRequest();
 
        UserRepositoryMock
            .Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
 
        var exception = await Assert.ThrowsAsync<EntityException>(() =>
            _handler.Handle(request, CancellationToken.None));
 
        Assert.Equal(EntityErrorType.NotFound, exception.ErrorType);
    }
}

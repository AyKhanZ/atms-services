using System.Linq.Expressions;
using ATMS.Admin.Contracts.Requests.Me;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Service.Handlers.Me;
using ATMS.Application.Models;
using ATMS.Exceptions.Entity;
using Moq;

namespace Admin.Services.Tests.Handlers.Me;

public class GetCurrentRolesHandlerTest : BaseHandlerTest
{
    private readonly GetCurrentRolesHandler _handler;
 
    public GetCurrentRolesHandlerTest()
    {
        _handler = new GetCurrentRolesHandler(UserRepositoryMock.Object, MapperMock.Object);
    }
 
    private GetCurrentRolesRequest CreateRequest(Guid? userId = null) =>
        new() { UserId = userId ?? Guid.NewGuid() };
 
    [Fact]
    public async Task Handle_WhenUserExists_ReturnsMappedRoles()
    {
        var request = CreateRequest();
        var roles = new List<Role> { new() { Id = Guid.NewGuid(), Name = "Admin" } };
        var expectedModels = new[] { new DictionaryModel<Guid> { Id = roles[0].Id, Name = roles[0].Name } };
 
        UserRepositoryMock
            .Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
 
        UserRepositoryMock
            .Setup(r => r.GetRolesAsync(request.UserId, It.IsAny<CancellationToken>()))
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
        UserRepositoryMock
            .Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
 
        var exception = await Assert.ThrowsAsync<EntityException>(() =>
            _handler.Handle(CreateRequest(), CancellationToken.None));
 
        Assert.Equal(EntityErrorType.NotFound, exception.ErrorType);
    }
}

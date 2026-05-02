using ATMS.Admin.Contracts.Models;
using ATMS.Admin.Contracts.Requests.Roles;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Service.Handlers.Roles;
using Moq;

namespace Admin.Services.Tests.Handlers.Roles;

public class GetRolesHandlerTest : BaseHandlerTest
{
    private readonly GetRolesHandler _handler;
 
    public GetRolesHandlerTest()
    {
        _handler = new GetRolesHandler(
            RoleRepositoryMock.Object,
            MapperMock.Object,
            CacheServiceMock.Object);
    }
 
    [Fact]
    public async Task Handle_ReturnsMappedRoles()
    {
        var roles = new List<Role> { new(), new() };
        var expectedModels = new RoleModel[] { new(), new() };
        
        CacheServiceMock
            .Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<RoleModel[]>>>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<Task<RoleModel[]>>, TimeSpan, CancellationToken>(
                (_, factory, _, _) => factory()!);
 
        RoleRepositoryMock
            .Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);
 
        MapperMock
            .Setup(m => m.Map<RoleModel[]>(roles))
            .Returns(expectedModels);
 
        var result = await _handler.Handle(new GetRolesRequest(), CancellationToken.None);
 
        Assert.Equal(expectedModels, result);
    }
 
    [Fact]
    public async Task Handle_WhenNoRoles_ReturnsEmptyArray()
    {
        RoleRepositoryMock
            .Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
 
        MapperMock
            .Setup(m => m.Map<RoleModel[]>(It.IsAny<Role[]>()))
            .Returns([]);
 
        var result = await _handler.Handle(new GetRolesRequest(), CancellationToken.None);
 
        Assert.Empty(result);
    }
}
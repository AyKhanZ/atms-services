using System.Linq.Expressions;
using ATMS.Admin.Contracts.Models;
using ATMS.Admin.Contracts.Requests.Roles;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Service.Handlers.Roles;
using ATMS.Application.Exceptions.Entity;
using Moq;

namespace Admin.Services.Tests.Handlers.Roles;

public class GetRoleHandlerTest : BaseHandlerTest
{
    private readonly GetRoleHandler _handler;

    public GetRoleHandlerTest()
    {
        _handler = new GetRoleHandler(RoleRepositoryMock.Object, MapperMock.Object);
    }

    [Fact]
    public async Task Handle_WhenRoleExists_ReturnsMappedModel()
    {
        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Description = "Test"
        };
        var expectedModel = new RoleModel
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Description = "Test"
        };
        var request = new GetRoleRequest
        {
            Id = Guid.NewGuid()
        };

        RoleRepositoryMock.Setup(r => r.GetAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);
        
        MapperMock
            .Setup(m => m.Map<RoleModel>(role))
            .Returns(expectedModel);
        
        var result = await _handler.Handle(request, CancellationToken.None);
        
        Assert.NotNull(result);
        Assert.Equal(expectedModel, result);
    }
    
    [Fact]
    public async Task Handle_WhenRoleNotFound_ThrowsEntityException()
    {
        RoleRepositoryMock
            .Setup(r => r.GetAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);
 
        var exception = await Assert.ThrowsAsync<EntityException>(() =>
            _handler.Handle(new GetRoleRequest { Id = Guid.NewGuid() }, CancellationToken.None));
 
        Assert.Equal(EntityErrorType.NotFound, exception.ErrorType);
    }
}

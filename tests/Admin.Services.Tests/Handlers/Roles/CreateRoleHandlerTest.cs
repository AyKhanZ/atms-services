using ATMS.Admin.Contracts.Commands.Role;
using ATMS.Admin.Contracts.Models;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Service.Handlers.Roles;
using Moq;

namespace Admin.Services.Tests.Handlers.Roles;

public class CreateRoleHandlerTest : BaseHandlerTest
{
    private readonly CreateRoleHandler _handler;
 
    public CreateRoleHandlerTest()
    {
        _handler = new CreateRoleHandler(MapperMock.Object, RoleRepositoryMock.Object);
    }
    
    [Fact]
    public async Task Handle_CreatesRolePermissions()
    {
        var command = new CreateRoleCommand { Name = "Admin", PermissionIds = [1, 2, 3] };
        var entity = new Role { Id = Guid.NewGuid() };

        MapperMock.Setup(m => m.Map<Role>(command)).Returns(entity);
        MapperMock.Setup(m => m.Map<RoleModel>(entity)).Returns(new RoleModel());

        await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(3, entity.RolePermissions.Count);
        Assert.All(entity.RolePermissions, rp => Assert.Equal(entity.Id, rp.RoleId));
        Assert.Contains(entity.RolePermissions, rp => rp.PermissionId == 1);
        Assert.Contains(entity.RolePermissions, rp => rp.PermissionId == 2);
        Assert.Contains(entity.RolePermissions, rp => rp.PermissionId == 3);
    }
    
    [Fact]
    public async Task Handle_DistinctPermissionIds_AreApplied()
    {
        var command = new CreateRoleCommand { Name = "Admin", PermissionIds = [1, 2, 2, 3] };
        var entity = new Role { Id = Guid.NewGuid() };

        MapperMock.Setup(m => m.Map<Role>(command)).Returns(entity);
        MapperMock.Setup(m => m.Map<RoleModel>(entity)).Returns(new RoleModel());

        await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(3, entity.RolePermissions.Count);
        Assert.Contains(entity.RolePermissions, rp => rp.PermissionId == 1);
        Assert.Contains(entity.RolePermissions, rp => rp.PermissionId == 2);
        Assert.Contains(entity.RolePermissions, rp => rp.PermissionId == 3);
    }
 
    [Fact]
    public async Task Handle_CreatesEntityAndReturnsMappedModel()
    {
        var command = new CreateRoleCommand { Name = "Admin", Description = "Admin role", PermissionIds = [1, 2, 3] };
        var entity = new Role { Name = command.Name };
        var expectedModel = new RoleModel { Name = command.Name };
 
        MapperMock.Setup(m => m.Map<Role>(command)).Returns(entity);
        MapperMock.Setup(m => m.Map<RoleModel>(entity)).Returns(expectedModel);
 
        var result = await _handler.Handle(command, CancellationToken.None);
 
        Assert.Equal(entity.Id, result);
        RoleRepositoryMock.Verify(r => r.CreateAsync(entity,
            It.IsAny<CancellationToken>()), Times.Once);
    }
 
    [Fact]
    public async Task Handle_SetsNewIdOnEntity()
    {
        var command = new CreateRoleCommand { Name = "Admin", PermissionIds = [1, 2, 3] };
        var entity = new Role { Id = Guid.Empty };
 
        MapperMock.Setup(m => m.Map<Role>(command)).Returns(entity);
        MapperMock.Setup(m => m.Map<RoleModel>(entity)).Returns(new RoleModel());
 
        await _handler.Handle(command, CancellationToken.None);
 
        Assert.NotEqual(Guid.Empty, entity.Id);
    }
}

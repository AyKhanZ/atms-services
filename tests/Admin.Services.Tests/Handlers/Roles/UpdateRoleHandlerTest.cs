using System.Linq.Expressions;
using ATMS.Admin.Contracts.Commands.Role;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Service.Handlers.Roles;
using ATMS.Application.Exceptions.Entity;
using Moq;

namespace Admin.Services.Tests.Handlers.Roles;

public class UpdateRoleHandlerTest : BaseHandlerTest
{
    private readonly UpdateRoleHandler _handler;
 
    public UpdateRoleHandlerTest()
    {
        _handler = new UpdateRoleHandler(RoleRepositoryMock.Object);
    }
 
    private UpdateRoleCommand CreateCommand(Guid? id = null) =>
        new() { Id = id ?? Guid.NewGuid(), Name = "Admin", PermissionIds = [1, 2, 3]  };

    private void SetupRoleExists(bool exists) =>
        RoleRepositoryMock
            .Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);

    [Fact]
    public async Task Handle_WhenRoleExists_CallsUpdateAsyncWithMappedEntity()
    {
        var command = CreateCommand();
        var entity = new Role { Id = command.Id, Name = command.Name };

        SetupRoleExists(true);
        RoleRepositoryMock.Setup(u => u.FindAsync(It.IsAny<Expression<Func<Role, bool>>>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(entity);

        await _handler.Handle(command, CancellationToken.None);

        RoleRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRoleNotFound_ThrowsEntityException()
    {
        var command = CreateCommand();
        SetupRoleExists(false);

        var exception = await Assert.ThrowsAsync<EntityException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Equal(EntityErrorType.NotFound, exception.ErrorType);
    }

    [Fact]
    public async Task Handle_WhenRoleNotFound_DoesNotCallSaveAsync()
    {
        var command = CreateCommand();
        SetupRoleExists(false);

        await Assert.ThrowsAsync<EntityException>(() =>
            _handler.Handle(command, CancellationToken.None));

        RoleRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task Handle_UpdatesSuccessfully()
    {
        var command = CreateCommand();
        var entity = new Role {
            Id = command.Id,
            Name = "OldName",
            Description = "OldDesc",
            RolePermissions = [ new RolePermission { PermissionId = 1, RoleId = command.Id }]
        };

        RoleRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(command.Name, entity.Name);
        Assert.Equal(command.Description, entity.Description);
        Assert.Contains(entity.RolePermissions, rp => rp.PermissionId == 2);
        Assert.Contains(entity.RolePermissions, rp => rp.PermissionId == 3);
    }
    
    [Fact]
    public async Task Handle_RemovesPermissionsNotInCommand()
    {
        var command = new UpdateRoleCommand { Id = Guid.NewGuid(), Name = "Admin", PermissionIds = [1] };
        var entity = new Role { Id = command.Id, Name = "Old", RolePermissions =
            [
                new RolePermission { PermissionId = 1, RoleId = command.Id },
                new RolePermission { PermissionId = 2, RoleId = command.Id },
                new RolePermission { PermissionId = 3, RoleId = command.Id },
                new RolePermission { PermissionId = 4, RoleId = command.Id },
                new RolePermission { PermissionId = 5, RoleId = command.Id }
            ]
        };

        RoleRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        await _handler.Handle(command, CancellationToken.None);

        Assert.Single(entity.RolePermissions);
        Assert.Equal(1, entity.RolePermissions.First().PermissionId);
    }
    
    [Fact]
    public async Task Handle_DistinctPermissionIds_AreApplied()
    {
        var command = new UpdateRoleCommand { Id = Guid.NewGuid(), Name = "Admin", PermissionIds = [1, 2, 2, 3] };
        var entity = new Role { Id = command.Id, Name = "Old", RolePermissions = [] };

        RoleRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(3, entity.RolePermissions.Count);
        Assert.Contains(entity.RolePermissions, rp => rp.PermissionId == 1);
        Assert.Contains(entity.RolePermissions, rp => rp.PermissionId == 2);
        Assert.Contains(entity.RolePermissions, rp => rp.PermissionId == 3);
    }
}

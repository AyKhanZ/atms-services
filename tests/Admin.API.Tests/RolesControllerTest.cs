using ATMS.Admin.API.Controllers.v1;
using ATMS.Admin.Contracts.Commands.Role;
using ATMS.Admin.Contracts.Models;
using ATMS.Admin.Contracts.Requests.Roles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Admin.API.Tests;

public class RolesControllerTest : BaseControllerTest
{
    private readonly RolesController _controller;

    public RolesControllerTest()
    {
        _controller = new RolesController(MediatorMock.Object);
    }

    [Fact]
    public async Task Index_ReturnsOkWithRoles()
    {
        // Arrange
        var roles = new[]
        {
            new RoleModel { Id = Guid.NewGuid(), UserType = 1, IsSystem = true, Name = "Admin" },
            new RoleModel { Id = Guid.NewGuid(), UserType = 1, IsSystem = true, Name = "User" },
            new RoleModel { Id = Guid.NewGuid(), UserType = 1, IsSystem = false, Name = "SuperAdmin" },
            new RoleModel { Id = Guid.NewGuid(), UserType = 1, IsSystem = false, Name = "Moderator" },
            new RoleModel { Id = Guid.NewGuid(), UserType = 1, IsSystem = false, Name = "Manager" },
            new RoleModel { Id = Guid.NewGuid(), UserType = 1, IsSystem = false, Name = "Guest" }
        };

        MediatorMock
            .Setup(m => m.Send(It.IsAny<GetRolesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);

        var request = new GetRolesRequest();

        // Act
        var result = await _controller.Index(request, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(roles, okResult.Value);
    }

    [Fact]
    public async Task Get_ReturnsOkWithRole()
    {
        // Arrange
        var roles = new[]
        {
            new RoleModel { Id = Guid.NewGuid(), UserType = 1, IsSystem = true, Name = "Admin" },
            new RoleModel { Id = Guid.NewGuid(), UserType = 1, IsSystem = true, Name = "User" },
            new RoleModel { Id = Guid.NewGuid(), UserType = 1, IsSystem = false, Name = "SuperAdmin" },
            new RoleModel { Id = Guid.NewGuid(), UserType = 1, IsSystem = false, Name = "Moderator" },
            new RoleModel { Id = Guid.NewGuid(), UserType = 1, IsSystem = false, Name = "Manager" },
            new RoleModel { Id = Guid.NewGuid(), UserType = 1, IsSystem = false, Name = "Guest" }
        };

        MediatorMock
            .Setup(m => m.Send(It.IsAny<GetRoleRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles[0]);

        // Act
        var result = await _controller.Get(roles[0].Id, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(roles[0], okResult.Value);
    }

    [Fact]
    public async Task Update_ReturnsNoContent()
    {
        // Arrange
        MediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateRoleCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(0));

        var command = new UpdateRoleCommand { Id = Guid.NewGuid(), Name = "UpdatedRole", PermissionIds = [1, 2, 3] };

        // Act
        var result = await _controller.Update(command, CancellationToken.None);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(StatusCodes.Status204NoContent, noContentResult.StatusCode);
    }
}
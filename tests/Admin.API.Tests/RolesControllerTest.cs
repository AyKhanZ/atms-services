using ATMS.Admin.API.Controllers.v1;
using ATMS.Admin.Contracts.Commands.Role;
using ATMS.Admin.Contracts.Models;
using ATMS.Admin.Contracts.Requests.Roles;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Admin.API.Tests;

public class RolesControllerTest
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly RolesController _controllerMock;
    
    public RolesControllerTest()
    {
        _mediatorMock = new Mock<IMediator>();
        _controllerMock = new RolesController(_mediatorMock.Object);
    }

    [Fact]
    public async Task Index_ReturnsOkWithRoles()
    {
        // Arrange
        var roles = new[]
        {
            new RoleModel { Id = Guid.NewGuid(), Name = "Admin" },
            new RoleModel { Id = Guid.NewGuid(), Name = "User" },
            new RoleModel { Id = Guid.NewGuid(), Name = "SuperAdmin" },
            new RoleModel { Id = Guid.NewGuid(), Name = "Moderator" },
            new RoleModel { Id = Guid.NewGuid(), Name = "Manager" },
            new RoleModel { Id = Guid.NewGuid(), Name = "Guest" }
        };
        
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetRolesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);

        var request = new GetRolesRequest();

        // Act
        var result = await _controllerMock.Index(request, CancellationToken.None);

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
            new RoleModel { Id = Guid.NewGuid(), Name = "Admin" },
            new RoleModel { Id = Guid.NewGuid(), Name = "User" },
            new RoleModel { Id = Guid.NewGuid(), Name = "SuperAdmin" },
            new RoleModel { Id = Guid.NewGuid(), Name = "Moderator" },
            new RoleModel { Id = Guid.NewGuid(), Name = "Manager" },
            new RoleModel { Id = Guid.NewGuid(), Name = "Guest" }
        };
        
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetRoleRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles[0]);

        var request = new GetRoleRequest
        {
            Id =  roles[0].Id
        };

        // Act
        var result = await _controllerMock.Get(request, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(roles[0], okResult.Value);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction()
    {
        var role = new RoleModel { Id = Guid.NewGuid(), Name = "NewAdmin" };
        
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CreateRoleCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);
        
        var command = new CreateRoleCommand { Name = "NewAdmin" };
        
        // Act
        var result = await _controllerMock.Create(command, CancellationToken.None);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        
        Assert.Equal(nameof(RolesController.Get), createdResult.ActionName);
        Assert.Equal(createdResult.StatusCode, StatusCodes.Status201Created);
    }
    
    [Fact]
    public async Task Update_ReturnsNoContent()
    {
        // Arrange
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateRoleCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(0));

        var command = new UpdateRoleCommand { Id = Guid.NewGuid(), Name = "UpdatedRole" };

        // Act
        var result = await _controllerMock.Update(command, CancellationToken.None);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(StatusCodes.Status204NoContent, noContentResult.StatusCode);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        // Arrange
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<DeleteRoleCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(0));

        var command = new DeleteRoleCommand { Id = Guid.NewGuid() };

        // Act
        var result = await _controllerMock.Delete(command, CancellationToken.None);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(StatusCodes.Status204NoContent, noContentResult.StatusCode);
    }
}

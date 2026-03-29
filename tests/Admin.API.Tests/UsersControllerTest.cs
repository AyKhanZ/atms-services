using ATMS.Admin.API.Controllers.v1;
using ATMS.Admin.Contracts.Models.Users;
using ATMS.Admin.Contracts.Requests.Users;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Admin.API.Tests;

public class UsersControllerTest
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly UsersController _controller;

    public UsersControllerTest()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new UsersController(_mediatorMock.Object);
    }
    
    [Fact]
    public async Task Index_ReturnsOkWithUsers()
    {
        // Arrange
        var users = new[]
        {
            new UserListItemModel { Id = Guid.NewGuid(), Name = "User test1" },
            new UserListItemModel { Id = Guid.NewGuid(), Name = "User test2" },
            new UserListItemModel { Id = Guid.NewGuid(), Name = "User test3" },
            new UserListItemModel { Id = Guid.NewGuid(), Name = "User test4" },
        };
        
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetUsersRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        var request = new GetUsersRequest();

        // Act
        var result = await _controller.Index(request, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(users, okResult.Value);
    }
    
    [Fact]
    public async Task Get_ReturnsOkWithUser()
    {
        // Arrange
        var users = new[]
        {
            new UserModel { Id = Guid.NewGuid(), Name = "User test1" },
            new UserModel { Id = Guid.NewGuid(), Name = "User test2" },
            new UserModel { Id = Guid.NewGuid(), Name = "User test3" },
            new UserModel { Id = Guid.NewGuid(), Name = "User test4" },
        };
        
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetUserRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(users[0]);

        // Act
        var result = await _controller.Get(users[0].Id, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(users[0], okResult.Value);
    }
}

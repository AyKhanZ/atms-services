using ATMS.Admin.API.Controllers.v1;
using ATMS.Admin.Contracts.Commands.Users;
using ATMS.Admin.Contracts.Models.Users;
using ATMS.Admin.Contracts.Requests.Users;
using ATMS.Data.Criterias;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Admin.API.Tests;

public class UsersControllerTest : BaseControllerTest
{
    private readonly UsersController _controller;

    public UsersControllerTest()
    {
        _controller = new UsersController(MediatorMock.Object);
    }
    
    [Fact]
    public async Task Index_ReturnsOkWithPagedResult()
    {
        // Arrange
        var pagedResult = new PagedResult<UserListItemModel>
        {
            Items =
            [
                new UserListItemModel { Id = Guid.NewGuid(), Name = "User test1" },
                new UserListItemModel { Id = Guid.NewGuid(), Name = "User test2" }
            ],
            TotalCount = 2,
            Page = 1,
            PageSize = 20
        };

        MediatorMock
            .Setup(m => m.Send(It.IsAny<GetUsersRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _controller.Index(new GetUsersRequest(), CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var value = Assert.IsType<PagedResult<UserListItemModel>>(okResult.Value);
        Assert.Equal(2, value.TotalCount);
        Assert.Equal(2, value.Items.Length);
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
        
        MediatorMock
            .Setup(m => m.Send(It.IsAny<GetUserRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(users[0]);

        // Act
        var result = await _controller.Get(users[0].Id, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(users[0], okResult.Value);
    }
    
    [Fact]
    public async Task UpdateUserStatus_ReturnsNoContent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUserStatusCommand { Id = userId, UserStatusId = 1 };

        MediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateUserStatusCommand>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdateUserStatus(userId, command, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);
        MediatorMock.Verify(m => m.Send(
            It.Is<UpdateUserStatusCommand>(c => c.Id == userId),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
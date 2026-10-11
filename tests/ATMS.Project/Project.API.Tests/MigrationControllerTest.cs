using ATMS.Project.API.Controllers.v1;
using ATMS.Project.Contracts.Commands.Migration;
using ATMS.Project.Contracts.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Project.API.Tests;

public class MigrationControllerTest : BaseControllerTest
{
    private readonly MigrationController _controller;

    public MigrationControllerTest()
    {
        _controller = new MigrationController(MediatorMock.Object);
    }

    [Fact]
    public async Task Up_ShouldReturnOkWithResult()
    {
        var command = new ApplyMigrationsCommand();
        var expected = new MigrationModel();

        MediatorMock
            .Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.Up(command, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(expected, okResult.Value);

        MediatorMock.Verify(
            m => m.Send(command, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Down_ShouldReturnOkWithResult()
    {
        var command = new DownMigrationCommand();
        var expected = new MigrationModel();

        MediatorMock
            .Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.Down(command, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(expected, okResult.Value);

        MediatorMock.Verify(
            m => m.Send(command, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

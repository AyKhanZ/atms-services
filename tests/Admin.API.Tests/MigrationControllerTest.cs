using ATMS.Admin.API.Controllers.v1;
using ATMS.Admin.Contracts.Commands.Migration;
using ATMS.Admin.Contracts.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Admin.API.Tests;

public class MigrationControllerTest
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly MigrationController _controller;

    public MigrationControllerTest()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new MigrationController(_mediatorMock.Object);
    }
    
    [Fact]
    public async Task Up_ShouldReturnOkWithResult()
    {
        // Arrange
        var command = new ApplyMigrationsCommand();
        var expected = new MigrationModel();

        _mediatorMock
            .Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _controller.Up(command, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(expected, okResult.Value);

        _mediatorMock.Verify(
            m => m.Send(command, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Down_ShouldReturnOkWithResult()
    {
        // Arrange
        var command = new DownMigrationCommand();
        var expected = new MigrationModel();

        _mediatorMock
            .Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _controller.Down(command, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(expected, okResult.Value);

        _mediatorMock.Verify(
            m => m.Send(command, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

using ATMS.Admin.API.Controllers.v1;
using ATMS.Admin.Contracts.Models.Me;
using ATMS.Admin.Contracts.Requests.Me;
using ATMS.Application.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Admin.API.Tests;

public class MeControllerTest : BaseControllerTest
{
    private readonly MeController _controller;

    public MeControllerTest()
    {
        _controller = new MeController(MediatorMock.Object);
    }

    [Fact]
    public async Task GetMeAsync_Should_Return_Ok_With_MeModel()
    {
        // Arrange
        var expected = new MeModel();

        MediatorMock
            .Setup(m => m.Send(
                It.IsAny<GetMeRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _controller.GetMeAsync(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(expected, okResult.Value);

        MediatorMock.Verify(m => m.Send(
                It.IsAny<GetMeRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetPermissionsAsync_Should_Return_Ok_With_StringArray()
    {
        // Arrange
        var expected = new[] { "perm.read", "perm.write" };

        MediatorMock
            .Setup(m => m.Send(
                It.IsAny<GetCurrentPermissionsRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _controller.GetPermissionsAsync(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(expected, okResult.Value);

        MediatorMock.Verify(m => m.Send(
                It.IsAny<GetCurrentPermissionsRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetRolesAsync_Should_Return_Ok_With_DictionaryModels()
    {
        // Arrange
        var expected = new[]
        {
            new DictionaryModel<Guid> { Id = Guid.NewGuid(), Name = "Admin" },
            new DictionaryModel<Guid> { Id = Guid.NewGuid(), Name = "User" }
        };

        MediatorMock
            .Setup(m => m.Send(
                It.IsAny<GetCurrentRolesRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _controller.GetRolesAsync(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(expected, okResult.Value);

        MediatorMock.Verify(m => m.Send(
                It.IsAny<GetCurrentRolesRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
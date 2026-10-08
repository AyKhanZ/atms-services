using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ATMS.Admin.API.Controllers.v1;
using ATMS.Admin.Contracts.Commands.Authentication;
using ATMS.Admin.Contracts.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Admin.API.Tests;

public class AuthenticationControllerTest : BaseControllerTest
{
    private readonly AuthenticationController _controller;

    public AuthenticationControllerTest()
    {
        _controller = new AuthenticationController(MediatorMock.Object);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnOkWithAccessInfo()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = Faker.Internet.Email(),
            Password = Faker.Internet.Password()
        };
        var expected = new AccessInfoModel();

        MediatorMock
            .Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _controller.LoginAsync(command, CancellationToken.None);

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);

        MediatorMock.Verify(
            m => m.Send(command, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnOkWithAccessInfo()
    {
        // Arrange
        var command = new RefreshTokenCommand
        {
            RefreshToken = Faker.Internet.Url()
        };
        var expected = new AccessInfoModel();

        MediatorMock
            .Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _controller.RefreshTokenAsync(command, CancellationToken.None);

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);

        MediatorMock.Verify(
            m => m.Send(command, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task LogoutAsync_ShouldSetUserIdAndReturnNoContent()
    {
        // Arrange
        var command = new LogoutCommand
        {
            RefreshToken = Faker.Internet.Url()
        };
        var userId = Guid.NewGuid();

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString())
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = user
            }
        };

        MediatorMock
            .Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.LogoutAsync(command, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);

        MediatorMock.Verify(
            m => m.Send(command, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
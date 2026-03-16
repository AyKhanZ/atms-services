using System.Security.Claims;
using ATMS.Admin.API.Controllers.v1;
using ATMS.Admin.Contracts.Commands.Authentication;
using ATMS.Admin.Contracts.Models;
using Bogus;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Admin.API.Tests;

public class AuthenticationControllerTest
{
    private readonly Faker _faker;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly AuthenticationController _controller;

    public AuthenticationControllerTest()
    {
        _faker = new Faker();
        _mediatorMock = new Mock<IMediator>();
        _controller = new AuthenticationController(_mediatorMock.Object);
    }
    
    [Fact]
    public async Task LoginAsync_ShouldReturnOkWithAccessInfo()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = _faker.Internet.Email(),
            Password = _faker.Internet.Password()
        };
        var expected = new AccessInfoModel();

        _mediatorMock
            .Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _controller.LoginAsync(command, CancellationToken.None);

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);

        _mediatorMock.Verify(
            m => m.Send(command, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnOkWithAccessInfo()
    {
        // Arrange
        var command = new RefreshTokenCommand
        {
            RefreshToken = _faker.Internet.Url()
        };
        var expected = new AccessInfoModel();

        _mediatorMock
            .Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _controller.RefreshTokenAsync(command, CancellationToken.None);

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);

        _mediatorMock.Verify(
            m => m.Send(command, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task LogoutAsync_ShouldSetUserIdAndReturnNoContent()
    {
        // Arrange
        var command = new LogoutCommand
        {
            UserId = Guid.NewGuid(),
            RefreshToken = _faker.Internet.Url()
        };
        var userId = Guid.NewGuid();
        
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
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

        _mediatorMock
            .Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.LogoutAsync(command, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);

        _mediatorMock.Verify(
            m => m.Send(command, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

using ATMS.Admin.API.Controllers.v1;
using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Contracts.Models.Users;
using Bogus;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Admin.API.Tests;

public class AccountControllerTest
{
    private readonly Faker _faker;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly IConfiguration _configuration;
    private readonly AccountController _controller;

    public AccountControllerTest()
    {
        _faker = new Faker();
        _mediatorMock = new Mock<IMediator>();
        _configuration = BuildConfiguration();
        _controller = new AccountController(_mediatorMock.Object, _configuration);
    }
    
    private static IConfiguration BuildConfiguration()
    {
        var settings = new Dictionary<string, string?>
        {
            ["RedirectUrlOptions:BaseUrl"] = "https://",
            ["RedirectUrlOptions:ResetPasswordPage"] = "https://reset",
            ["RedirectUrlOptions:EmailConfirmedPage"] = "https://ok",
            ["RedirectUrlOptions:EmailConfirmFailedPage"] = "https://fail"
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
    }
    
    [Fact]
    public async Task Register_ShouldReturnCreatedAtAction()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = _faker.Internet.Email(),
            Surname = _faker.Name.FirstName(),
            Name = _faker.Name.LastName(),
            RoleId = _faker.Random.Guid(),
        };
        var user = new UserModel { Id = Guid.NewGuid() };

        _mediatorMock
            .Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.Register(command, CancellationToken.None);

        // Assert
        Assert.IsType<CreatedAtActionResult>(result.Result);

        _mediatorMock.Verify(
            m => m.Send(command, It.IsAny<CancellationToken>()),
            Times.Once);
    }
    
    [Fact]
    public async Task ConfirmEmail_ShouldRedirectToSuccessWhenConfirmed()
    {
        // Arrange
        var expectedPage = "https://ok";
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<ConfirmEmailCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ConfirmEmail(_faker.Internet.Url(), CancellationToken.None);

        // Assert
        
        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal(expectedPage, redirect.Url);
    }

    [Fact]
    public async Task ConfirmEmail_ShouldRedirectToFailWhenNotConfirmed()
    {
        // Arrange
        var expectedPage = "https://fail";

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<ConfirmEmailCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.ConfirmEmail(_faker.Internet.Url(), CancellationToken.None);

        // Assert
        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal(expectedPage, redirect.Url);
    }

    [Fact]
    public async Task ResendConfirmationLetter_ShouldReturnAccepted()
    {
        // Arrange
        var command = new ResendEmailConfirmationCommand
        {
            Email = _faker.Internet.Email()
        };

        _mediatorMock
            .Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.ResendConfirmationLetter(command, CancellationToken.None);

        // Assert
        Assert.IsType<AcceptedResult>(result);

        _mediatorMock.Verify(
            m => m.Send(command, It.IsAny<CancellationToken>()),
            Times.Once);
    }
    
    [Fact]
    public async Task ChangePassword_ShouldReturnNoContent()
    {
        // Arrange
        var command = new ChangePasswordCommand
        {
            Email = _faker.Internet.Email(),
            NewPassword = _faker.Internet.Password(),
            OldPassword = _faker.Internet.Password()
        };

        _mediatorMock
            .Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.ChangePassword(command, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);

        _mediatorMock.Verify(
            m => m.Send(command, It.IsAny<CancellationToken>()),
            Times.Once);
    }
    
    [Fact]
    public async Task ForgotPassword_ShouldReturnAccepted()
    {
        // Arrange
        var command = new ForgotPasswordCommand
        {
            Email = _faker.Internet.Email()
        };

        _mediatorMock
            .Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.ForgotPassword(command, CancellationToken.None);

        // Assert
        Assert.IsType<AcceptedResult>(result);

        _mediatorMock.Verify(
            m => m.Send(command, It.IsAny<CancellationToken>()),
            Times.Once);
    }
    
    [Fact]
    public async Task ResetPassword_ShouldReturnNoContent()
    {
        // Arrange
        var command = new ResetPasswordCommand
        {
            ConfirmPassword = _faker.Internet.Password(),
            Password = _faker.Internet.Password(),
            Token = _faker.Internet.Url()
        };

        _mediatorMock
            .Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.ResetPassword(command, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);

        _mediatorMock.Verify(
            m => m.Send(command, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

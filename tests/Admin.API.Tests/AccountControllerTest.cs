using ATMS.Admin.API.Controllers.v1;
using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Contracts.Enums;
using ATMS.Admin.Contracts.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Admin.API.Tests;

public class AccountControllerTest : BaseControllerTest
{
    private readonly AccountController _controller;

    public AccountControllerTest()
    {
        var configuration = BuildConfiguration();
        _controller = new AccountController(MediatorMock.Object, configuration);
    }

    [Fact]
    public async Task Register_ShouldReturnCreatedAtAction()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = Faker.Internet.Email(),
            Surname = Faker.Name.FirstName(),
            Name = Faker.Name.LastName(),
            RoleId = Faker.Random.Guid(),
        };
        var user = new UserModel { Id = Guid.NewGuid() };

        MediatorMock
            .Setup(m => m.Send(command,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.Register(command, CancellationToken.None);

        // Assert
        Assert.IsType<CreatedAtActionResult>(result.Result);

        MediatorMock.Verify(
            m => m.Send(command, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ConfirmEmail_ShouldRedirectToSuccessWhenConfirmed()
    {
        // Arrange
        var expectedPage = "https://ok";
        MediatorMock
            .Setup(m => m.Send(It.IsAny<ConfirmEmailCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConfirmEmailResultEnum.Confirmed);

        // Act
        var result = await _controller.ConfirmEmail(Faker.Internet.Url(), CancellationToken.None);

        // Assert

        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal(expectedPage, redirect.Url);
    }

    [Fact]
    public async Task ConfirmEmail_ShouldRedirectToAlreadyConfirmedWhenEmailWasConfirmedBefore()
    {
        // Arrange
        var expectedPage = "https://already-ok";
        MediatorMock
            .Setup(m => m.Send(It.IsAny<ConfirmEmailCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConfirmEmailResultEnum.AlreadyConfirmed);

        // Act
        var result = await _controller.ConfirmEmail(Faker.Internet.Url(), CancellationToken.None);

        // Assert

        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal(expectedPage, redirect.Url);
    }

    [Fact]
    public async Task ConfirmEmail_ShouldRedirectToFailWhenNotConfirmed()
    {
        // Arrange
        var expectedPage = "https://fail";

        MediatorMock
            .Setup(m => m.Send(It.IsAny<ConfirmEmailCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConfirmEmailResultEnum.Failed);

        // Act
        var result = await _controller.ConfirmEmail(Faker.Internet.Url(), CancellationToken.None);

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
            Email = Faker.Internet.Email()
        };

        MediatorMock
            .Setup(m => m.Send(command,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.ResendConfirmationLetter(command, CancellationToken.None);

        // Assert
        Assert.IsType<AcceptedResult>(result);

        MediatorMock.Verify(
            m => m.Send(command,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }


    [Fact]
    public void ResendConfirmationLetter_ShouldAllowAnonymous()
    {
        var method = typeof(AccountController).GetMethod(nameof(AccountController.ResendConfirmationLetter));

        Assert.NotNull(method);
        Assert.Contains(method.GetCustomAttributes(typeof(AllowAnonymousAttribute), false),
            attribute => attribute is AllowAnonymousAttribute);
    }
    [Fact]
    public async Task ChangePassword_ShouldReturnNoContent()
    {
        // Arrange
        var command = new ChangePasswordCommand
        {
            Email = Faker.Internet.Email(),
            NewPassword = Faker.Internet.Password(),
            OldPassword = Faker.Internet.Password()
        };

        MediatorMock
            .Setup(m => m.Send(command,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.ChangePassword(command, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);

        MediatorMock.Verify(
            m => m.Send(command, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ForgotPassword_ShouldReturnAccepted()
    {
        // Arrange
        var command = new ForgotPasswordCommand
        {
            Email = Faker.Internet.Email()
        };

        MediatorMock
            .Setup(m => m.Send(command,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.ForgotPassword(command, CancellationToken.None);

        // Assert
        Assert.IsType<AcceptedResult>(result);

        MediatorMock.Verify(
            m => m.Send(command, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ResetPassword_ShouldReturnNoContent()
    {
        // Arrange
        var command = new ResetPasswordCommand
        {
            ConfirmPassword = Faker.Internet.Password(),
            Password = Faker.Internet.Password(),
            Token = Faker.Internet.Url()
        };

        MediatorMock
            .Setup(m => m.Send(command,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.ResetPassword(command, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);

        MediatorMock.Verify(
            m => m.Send(command, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

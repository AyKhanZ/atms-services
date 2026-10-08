using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Security.Claims;
using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Contracts.Enums;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Service.Handlers.Account;
using Moq;

namespace Admin.Services.Tests.Handlers.Account;

public class ConfirmEmailHandlerTest : BaseHandlerTest
{
    private readonly ConfirmEmailHandler _handler;
 
    public ConfirmEmailHandlerTest()
    {
        _handler = new ConfirmEmailHandler(EmailConfirmationTokenServiceMock.Object, UserRepositoryMock.Object);
    }
 
    private ConfirmEmailCommand CreateCommand(string token = "valid-token") => new() { Token = token };
 
    private ClaimsPrincipal CreatePrincipal(Guid userId) =>
        new(new ClaimsIdentity([
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString())
        ]));
 
    [Fact]
    public async Task Handle_WhenTokenInvalid_ReturnsFalse()
    {
        EmailConfirmationTokenServiceMock
            .Setup(s => s.ValidateTokenAsync(It.IsAny<string>()))
            .ReturnsAsync((ClaimsPrincipal?)null);
 
        var result = await _handler.Handle(CreateCommand(), CancellationToken.None);
 
        Assert.Equal(ConfirmEmailResultEnum.Failed, result);
    }
 
    [Fact]
    public async Task Handle_WhenUserIdClaimInvalid_ReturnsFalse()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, "not-a-guid")
        ]));
 
        EmailConfirmationTokenServiceMock
            .Setup(s => s.ValidateTokenAsync(It.IsAny<string>()))
            .ReturnsAsync(principal);
 
        var result = await _handler.Handle(CreateCommand(), CancellationToken.None);
 
        Assert.Equal(ConfirmEmailResultEnum.Failed, result);
    }
 
    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsFalse()
    {
        var userId = Guid.NewGuid();

        EmailConfirmationTokenServiceMock
            .Setup(s => s.ValidateTokenAsync(It.IsAny<string>()))
            .ReturnsAsync(CreatePrincipal(userId));

        UserRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _handler.Handle(CreateCommand(), CancellationToken.None);

        Assert.Equal(ConfirmEmailResultEnum.Failed, result);
    }

    [Fact]
    public async Task Handle_WhenEmailAlreadyConfirmed_ReturnsAlreadyConfirmed()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, EmailConfirmed = true };

        EmailConfirmationTokenServiceMock
            .Setup(s => s.ValidateTokenAsync(It.IsAny<string>()))
            .ReturnsAsync(CreatePrincipal(userId));

        UserRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _handler.Handle(CreateCommand(), CancellationToken.None);

        Assert.Equal(ConfirmEmailResultEnum.AlreadyConfirmed, result);
        UserRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
 
    [Fact]
    public async Task Handle_WhenEmailNotConfirmed_ConfirmsEmailAndReturnsConfirmed()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, EmailConfirmed = false };
 
        EmailConfirmationTokenServiceMock
            .Setup(s => s.ValidateTokenAsync(It.IsAny<string>()))
            .ReturnsAsync(CreatePrincipal(userId));
 
        UserRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
 
        var result = await _handler.Handle(CreateCommand(), CancellationToken.None);
 
        Assert.Equal(ConfirmEmailResultEnum.Confirmed, result);
        Assert.True(user.EmailConfirmed);
        UserRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

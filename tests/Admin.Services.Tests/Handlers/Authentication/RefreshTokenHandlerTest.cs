using System.Linq.Expressions;
using ATMS.Admin.Contracts.Commands.Authentication;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Service.Exceptions.Auth;
using ATMS.Admin.Service.Handlers.Authentication;
using ATMS.Admin.Service.Security.Models;
using Moq;

namespace Admin.Services.Tests.Handlers.Authentication;

public class RefreshTokenHandlerTest : BaseHandlerTest
{
    private readonly RefreshTokenHandler _handler;
 
    private const string FakeRefreshToken = "fake-refresh-token";
    private const string FakeNewRefreshToken = "fake-new-refresh-token";
    private const string FakeAccessToken = "fake-access-token";
 
    public RefreshTokenHandlerTest()
    {
        _handler = new RefreshTokenHandler(
            AccessTokenServiceMock.Object,
            RefreshTokenServiceMock.Object,
            UserRepositoryMock.Object,
            BlackListServiceMock.Object);
    }
 
    private RefreshTokenCommand CreateCommand(string? refreshToken = null) =>
        new() { RefreshToken = refreshToken ?? FakeRefreshToken };
 
    [Fact]
    public async Task Handle_WhenTokenNotRevoked_ReturnsNewTokens()
    {
        var user = new User { Id = Guid.NewGuid() };
        var command = CreateCommand();
 
        BlackListServiceMock
            .Setup(s => s.IsRefreshTokenRevokedAsync(command.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
 
        UserRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
 
        AccessTokenServiceMock
            .Setup(s => s.GenerateTokenAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccessTokenResult(FakeAccessToken, DateTime.UtcNow.AddMinutes(60)));
 
        RefreshTokenServiceMock
            .Setup(s => s.GenerateTokenAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(FakeNewRefreshToken);
 
        var result = await _handler.Handle(command, CancellationToken.None);
 
        Assert.Equal(FakeAccessToken, result.AccessToken);
        Assert.Equal(FakeNewRefreshToken, result.RefreshToken);
    }
 
    [Fact]
    public async Task Handle_WhenTokenRevoked_ThrowsAuthException()
    {
        var command = CreateCommand();
 
        BlackListServiceMock
            .Setup(s => s.IsRefreshTokenRevokedAsync(command.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
 
        var exception = await Assert.ThrowsAsync<AuthException>(() =>
            _handler.Handle(command, CancellationToken.None));
 
        Assert.Equal(AuthErrorType.InvalidToken, exception.AuthErrorType);
    }
}

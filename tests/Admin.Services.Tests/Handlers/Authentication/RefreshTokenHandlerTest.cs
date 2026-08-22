using System.Linq.Expressions;
using ATMS.Admin.Contracts.Commands.Authentication;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Service.Handlers.Authentication;
using ATMS.Admin.Service.Security.Models;
using ATMS.Application.Exceptions.Auth;
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
    public async Task Handle_WhenTokenValid_ReturnsNewTokens()
    {
        var user = new User
        {
            Id = Guid.NewGuid(), RefreshToken = FakeRefreshToken, RefreshTokenExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };
        var command = CreateCommand();

        BlackListServiceMock.Setup(s => s.IsRefreshTokenRevokedAsync(command.RefreshToken,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        UserRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        AccessTokenServiceMock.Setup(s => s.GenerateTokenAsync(user,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccessTokenResult(FakeAccessToken, DateTime.UtcNow.AddMinutes(1)));

        RefreshTokenServiceMock.Setup(s => s.GenerateTokenAsync(user,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(FakeNewRefreshToken);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(FakeAccessToken, result.AccessToken);
        Assert.Equal(FakeNewRefreshToken, result.RefreshToken);
        BlackListServiceMock.Verify(s => s.TryAddToListAsync(user.Id, user.RefreshToken, user.RefreshTokenExpiresAt.Value,
            It.IsAny<CancellationToken>()), Times.Once);
        UserRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenTokenExpired_ThrowsAuthException()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            RefreshToken = FakeRefreshToken,
            RefreshTokenExpiresAt = DateTime.UtcNow.AddMinutes(-1)
        };
        var command = CreateCommand();

        UserRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        BlackListServiceMock.Setup(s => s.IsRefreshTokenRevokedAsync(It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<AuthException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenRefreshTokenNull_ThrowsAuthException()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            RefreshToken = null,
            RefreshTokenExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };
        var command = CreateCommand();

        UserRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        BlackListServiceMock.Setup(s => s.IsRefreshTokenRevokedAsync(It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<AuthException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenRefreshTokenExpiresAtNull_ThrowsAuthException()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            RefreshToken = FakeRefreshToken,
            RefreshTokenExpiresAt = null
        };
        var command = CreateCommand();

        UserRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        BlackListServiceMock.Setup(s => s.IsRefreshTokenRevokedAsync(It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<AuthException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ThrowsAuthException()
    {
        var command = CreateCommand();

        UserRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<AuthException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenTokenRevoked_ThrowsAuthException()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            RefreshToken = FakeRefreshToken,
            RefreshTokenExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };
        var command = CreateCommand();

        UserRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        BlackListServiceMock.Setup(s => s.IsRefreshTokenRevokedAsync(command.RefreshToken,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<AuthException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenTokenValid_SetsCorrectAccessTokenExpireTime()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            RefreshToken = FakeRefreshToken,
            RefreshTokenExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };
        var command = CreateCommand();

        BlackListServiceMock.Setup(s => s.IsRefreshTokenRevokedAsync(command.RefreshToken,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        UserRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var expectedExpiry = DateTime.UtcNow.AddMinutes(10);
        AccessTokenServiceMock.Setup(s => s.GenerateTokenAsync(user,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccessTokenResult(FakeAccessToken, expectedExpiry));

        RefreshTokenServiceMock.Setup(s => s.GenerateTokenAsync(user,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(FakeNewRefreshToken);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(expectedExpiry, result.AccessTokenExpireTime);
    }

    [Fact]
    public async Task Handle_WhenRefreshTokenWasConcurrentlyRevoked_ThrowsAuthException()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            RefreshToken = FakeRefreshToken,
            RefreshTokenExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };
        var command = CreateCommand();

        UserRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        BlackListServiceMock.Setup(s => s.IsRefreshTokenRevokedAsync(command.RefreshToken,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        AccessTokenServiceMock.Setup(s => s.GenerateTokenAsync(user,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccessTokenResult(FakeAccessToken, DateTime.UtcNow.AddMinutes(1)));

        RefreshTokenServiceMock.Setup(s => s.GenerateTokenAsync(user,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(FakeNewRefreshToken);

        BlackListServiceMock.Setup(s => s.TryAddToListAsync(
                user.Id,
                FakeRefreshToken,
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var exception = await Assert.ThrowsAsync<AuthException>(
            () => _handler.Handle(command, CancellationToken.None));

        Assert.Equal(AuthErrorType.InvalidToken, exception.AuthErrorType);
        UserRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}

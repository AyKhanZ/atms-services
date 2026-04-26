using System.Linq.Expressions;
using ATMS.Admin.Contracts.Commands.Authentication;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Service.Handlers.Authentication;
using ATMS.Application.Exceptions.Auth;
using Moq;

namespace Admin.Services.Tests.Handlers.Authentication;

public class LogoutHandlerTest : BaseHandlerTest
{
    private readonly LogoutHandler _handler;
 
    public LogoutHandlerTest()
    {
        _handler = new LogoutHandler(UserRepositoryMock.Object, CurrentUserMock.Object, BlackListServiceMock.Object);
    }

    private LogoutCommand CreateCommand(Guid? userId = null, string? refreshToken = null)
    {
        return new LogoutCommand
        {
            RefreshToken = refreshToken ?? "valid-refresh-token"
        };
    }
    
    [Fact]
    public async Task Handle_WhenTokenNotRevoked_ResetsUserTokensAndSaves()
    {
        var command = CreateCommand();
        var user = new User
        {
            RefreshToken = command.RefreshToken,
            RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        BlackListServiceMock
            .Setup(s => s.IsRefreshTokenRevokedAsync(command.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        UserRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await _handler.Handle(command, CancellationToken.None);

        Assert.Null(user.RefreshToken);
        Assert.Null(user.RefreshTokenExpiresAt);
        UserRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
        
    [Fact]
    public async Task Handle_WhenTokenNotRevoked_AddsUserToBlackList()
    {
        var command = CreateCommand();
        var user = new User {
            RefreshToken = command.RefreshToken,
            RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7)
        };
 
        BlackListServiceMock
            .Setup(s => s.IsRefreshTokenRevokedAsync(command.RefreshToken,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
 
        UserRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
 
        await _handler.Handle(command, CancellationToken.None);
 
        BlackListServiceMock.Verify(s => s.AddToListAsync(
            It.IsAny<Guid>(), 
            It.IsAny<string>(), 
            It.IsAny<DateTime>(), 
            It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task Handle_WhenUserHasNoRefreshToken_ThrowsAuthException()
    {
        var command = CreateCommand();
        var user = new User
        {
            RefreshToken = null,
            RefreshTokenExpiresAt = null
        };

        BlackListServiceMock
            .Setup(s => s.IsRefreshTokenRevokedAsync(command.RefreshToken,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        UserRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var exception = await Assert.ThrowsAsync<AuthException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Equal(AuthErrorType.InvalidToken, exception.AuthErrorType);
    }
 
    [Fact]
    public async Task Handle_WhenTokenRevoked_ThrowsAuthException()
    {
        var command = CreateCommand();
 
        BlackListServiceMock
            .Setup(s => s.IsRefreshTokenRevokedAsync(command.RefreshToken,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
 
        var exception = await Assert.ThrowsAsync<AuthException>(() =>
            _handler.Handle(command, CancellationToken.None));
 
        Assert.Equal(AuthErrorType.InvalidToken, exception.AuthErrorType);
    }
    
    [Fact]
    public async Task Handle_WhenUserNotFound_ThrowsAuthException()
    {
        var command = CreateCommand();

        BlackListServiceMock
            .Setup(s => s.IsRefreshTokenRevokedAsync(command.RefreshToken,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        UserRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var exception = await Assert.ThrowsAsync<AuthException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Equal(AuthErrorType.InvalidToken, exception.AuthErrorType);
    }

    [Fact]
    public async Task Handle_WhenTokenRevoked_DoesNotCallAddToBlackList()
    {
        var command = CreateCommand();

        BlackListServiceMock
            .Setup(s => s.IsRefreshTokenRevokedAsync(command.RefreshToken,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<AuthException>(() =>
            _handler.Handle(command, CancellationToken.None));

        BlackListServiceMock.Verify(s => s.AddToListAsync(
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<DateTime>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_DoesNotCallAddToBlackList()
    {
        var command = CreateCommand();

        BlackListServiceMock
            .Setup(s => s.IsRefreshTokenRevokedAsync(command.RefreshToken,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        UserRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<AuthException>(() =>
            _handler.Handle(command, CancellationToken.None));

        BlackListServiceMock.Verify(s => s.AddToListAsync(
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<DateTime>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }
}

using ATMS.Admin.Contracts.Commands.Authentication;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Service.Exceptions.Auth;
using ATMS.Admin.Service.Handlers.Authentication;
using Moq;

namespace Admin.Services.Tests.Handlers.Authentication;

public class LogoutHandlerTest : BaseHandlerTest
{
    private readonly LogoutHandler _handler;
 
    public LogoutHandlerTest()
    {
        _handler = new LogoutHandler(UserRepositoryMock.Object, BlackListServiceMock.Object);
    }

    private LogoutCommand CreateCommand(Guid? userId = null, string? refreshToken = null)
    {
        return new LogoutCommand
        {
            UserId = userId ?? Guid.NewGuid(),
            RefreshToken = refreshToken ?? "valid-refresh-token"
        };
    }
        
    [Fact]
    public async Task Handle_WhenTokenNotRevoked_AddsUserToBlackList()
    {
        var command = CreateCommand();
        var user = new User { Id = command.UserId };
 
        BlackListServiceMock
            .Setup(s => s.IsRefreshTokenRevokedAsync(command.RefreshToken,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
 
        UserRepositoryMock
            .Setup(r => r.GetAsync(command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
 
        await _handler.Handle(command, CancellationToken.None);
 
        BlackListServiceMock.Verify(s => s.AddToListAsync(user,
            It.IsAny<CancellationToken>()), Times.Once);
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
            .Setup(r => r.GetAsync(command.UserId, It.IsAny<CancellationToken>()))
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
            It.IsAny<User>(),
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
            .Setup(r => r.GetAsync(command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<AuthException>(() =>
            _handler.Handle(command, CancellationToken.None));

        BlackListServiceMock.Verify(s => s.AddToListAsync(
            It.IsAny<User>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }
}

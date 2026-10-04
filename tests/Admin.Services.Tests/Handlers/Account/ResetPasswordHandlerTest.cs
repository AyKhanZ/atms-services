using System.Linq.Expressions;
using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Entities.Tokens;
using ATMS.Admin.Service.Handlers.Account;
using ATMS.Application.Exceptions.Auth;
using ATMS.Application.Exceptions.Entity;
using Moq;

namespace Admin.Services.Tests.Handlers.Account;

public class ResetPasswordHandlerTest : BaseHandlerTest
{
    private readonly ResetPasswordHandler _handler;

    private const string FakeToken = "fake-reset-token";
    private const string FakePasswordHash = "hashed-password";

    public ResetPasswordHandlerTest()
    {
        _handler = new ResetPasswordHandler(
            PasswordResetTokenRepositoryMock.Object,
            UserRepositoryMock.Object,
            PasswordHasherServiceMock.Object);

        PasswordHasherServiceMock
            .Setup(p => p.Hash(It.IsAny<string>()))
            .Returns(FakePasswordHash);
        UserRepositoryMock
            .Setup(r => r.TrySavePasswordChangeAsync(It.IsAny<User>(), It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    private ResetPasswordCommand CreateCommand(string? token = null)
    {
        return new ResetPasswordCommand
        {
            Token = token ?? FakeToken,
            Password = "NewPass1!",
            ConfirmPassword = "NewPass1!"
        };
    }

    [Fact]
    public async Task Handle_WhenTokenValid_UpdatesPasswordAndClearsTokens()
    {
        var userId = Guid.NewGuid();
        var tokenEntity = new PasswordResetToken
        {
            Token = FakeToken,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };
        var user = new User { Id = userId, PasswordHash = "old-hash" };

        PasswordResetTokenRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<PasswordResetToken, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(tokenEntity);

        UserRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await _handler.Handle(CreateCommand(), CancellationToken.None);

        Assert.Equal(FakePasswordHash, user.PasswordHash);
        // Raised so that a session created in the same instant by a sign-in with the old password
        // still stops refreshing.
        Assert.Equal(1, user.SessionVersion);
        PasswordResetTokenRepositoryMock.Verify(r => r.StageConsume(tokenEntity), Times.Once);
        PasswordResetTokenRepositoryMock.Verify(
            r => r.ClearListAsync(It.IsAny<Expression<Func<PasswordResetToken, bool>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
        UserRepositoryMock.Verify(r => r.TrySavePasswordChangeAsync(It.IsAny<User>(), It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ClearsTemporaryLockout()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            PasswordHash = "old-hash",
            UserStatusId = (int)ATMS.Data.Enums.UserStatusEnum.Locked,
            LockoutEnd = DateTime.UtcNow.AddMinutes(10),
            FailedLoginCount = 3
        };
        SetupValidToken(userId, user);

        await _handler.Handle(CreateCommand(), CancellationToken.None);

        Assert.Equal((int)ATMS.Data.Enums.UserStatusEnum.Active, user.UserStatusId);
        Assert.Null(user.LockoutEnd);
        Assert.Equal((uint)0, user.FailedLoginCount);
    }

    private void SetupValidToken(Guid userId, User user)
    {
        PasswordResetTokenRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<PasswordResetToken, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PasswordResetToken
            {
                Token = FakeToken,
                UserId = userId,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10)
            });
        UserRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
    }

    [Fact]
    public async Task Handle_WhenTokenNotFound_ThrowsAuthException()
    {
        PasswordResetTokenRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<PasswordResetToken, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((PasswordResetToken?)null);

        var exception = await Assert.ThrowsAsync<AuthException>(() =>
            _handler.Handle(CreateCommand(), CancellationToken.None));

        Assert.Equal(AuthErrorType.InvalidToken, exception.AuthErrorType);
    }

    [Fact]
    public async Task Handle_WhenTokenExpired_ThrowsAuthException()
    {
        var tokenEntity = new PasswordResetToken
        {
            Token = FakeToken,
            UserId = Guid.NewGuid(),
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1)
        };

        PasswordResetTokenRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<PasswordResetToken, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(tokenEntity);

        var exception = await Assert.ThrowsAsync<AuthException>(() =>
            _handler.Handle(CreateCommand(), CancellationToken.None));

        Assert.Equal(AuthErrorType.InvalidToken, exception.AuthErrorType);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ThrowsEntityException()
    {
        var userId = Guid.NewGuid();
        var tokenEntity = new PasswordResetToken
        {
            Token = FakeToken,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };

        PasswordResetTokenRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<PasswordResetToken, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(tokenEntity);

        UserRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var exception = await Assert.ThrowsAsync<EntityException>(() =>
            _handler.Handle(CreateCommand(), CancellationToken.None));

        Assert.Equal(EntityErrorType.NotFound, exception.ErrorType);
    }

    // A lock with no end date was set by hand; resetting the password must not lift it.
    [Fact]
    public async Task Handle_ManualLock_StaysLocked()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            PasswordHash = "old-hash",
            UserStatusId = (int)ATMS.Data.Enums.UserStatusEnum.Locked,
            LockoutEnd = null
        };
        SetupValidToken(userId, user);

        await _handler.Handle(CreateCommand(), CancellationToken.None);

        Assert.Equal((int)ATMS.Data.Enums.UserStatusEnum.Locked, user.UserStatusId);
    }

    // The same link sent twice: the reset that commits second finds the version moved and the link
    // counts as used.
    [Fact]
    public async Task Handle_ConcurrentResetWonTheRace_RefusesAsUsedLink()
    {
        var userId = Guid.NewGuid();
        SetupValidToken(userId, new User { Id = userId, PasswordHash = "old-hash" });
        UserRepositoryMock
            .Setup(r => r.TrySavePasswordChangeAsync(It.IsAny<User>(), It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var error = await Assert.ThrowsAsync<AuthException>(() => _handler.Handle(CreateCommand(), CancellationToken.None));

        Assert.Equal(AuthErrorType.InvalidToken, error.AuthErrorType);
    }
}

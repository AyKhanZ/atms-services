using System.Linq.Expressions;
using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Entities.Tokens;
using ATMS.Admin.Service.Exceptions.Auth;
using ATMS.Admin.Service.Handlers.Account;
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
        PasswordResetTokenRepositoryMock.Verify(
            r => r.ClearListAsync(It.IsAny<Expression<Func<PasswordResetToken, bool>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
        UserRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
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
}
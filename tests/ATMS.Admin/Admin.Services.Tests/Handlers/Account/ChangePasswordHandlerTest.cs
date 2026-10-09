using System.Linq.Expressions;
using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Service.Handlers.Account;
using ATMS.Admin.Service.Security.Models;
using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Exceptions.Auth;
using ATMS.Admin.Service.Resources;
using ATMS.Application.Exceptions.Enums;
using ATMS.Data.Enums;
using FluentValidation;
using Moq;

namespace Admin.Services.Tests.Handlers.Account;

public class ChangePasswordHandlerTest : BaseHandlerTest
{
    public ChangePasswordHandlerTest()
    {
        UserRepositoryMock.Setup(x => x.TrySavePasswordChangeAsync(It.IsAny<User>(), It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
    }

    private ChangePasswordHandler CreateHandler() => new(
        CurrentUserMock.Object,
        UserRepositoryMock.Object,
        UserSessionRepositoryMock.Object,
        PasswordHasherServiceMock.Object,
        AccessTokenServiceMock.Object,
        RefreshTokenServiceMock.Object);

    private static ChangePasswordCommand Command() => new()
    {
        OldPassword = "OldPass123!",
        NewPassword = "NewPass123!",
        ConfirmPassword = "NewPass123!"
    };

    [Fact]
    public async Task Handle_ValidCurrentPassword_ReplacesSessionsAndReturnsTokens()
    {
        var user = new User
        {
            Id = Guid.NewGuid(), PasswordHash = "old-hash",
            UserStatusId = (int)UserStatusEnum.Active, FailedLoginCount = 3, SessionVersion = 2
        };
        CurrentUserMock.SetupGet(x => x.Id).Returns(user.Id);
        UserRepositoryMock.Setup(x => x.FindAsync(
                It.Is<Expression<Func<User, bool>>>(predicate =>
                    predicate.Compile()(user) && !predicate.Compile()(new User { Id = Guid.NewGuid() })),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        PasswordHasherServiceMock.Setup(x => x.Verify("OldPass123!", "old-hash")).Returns(true);
        PasswordHasherServiceMock.Setup(x => x.Hash("NewPass123!")).Returns("new-hash");
        AccessTokenServiceMock.Setup(x => x.GenerateTokenAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccessTokenResult("access", DateTime.UtcNow.AddMinutes(10)));
        RefreshTokenServiceMock.Setup(x => x.GenerateTokenAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RefreshTokenResult("refresh", "hash", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(7)));

        var result = await CreateHandler().Handle(Command(), CancellationToken.None);

        Assert.Equal("new-hash", user.PasswordHash);
        Assert.Equal("access", result.AccessToken);
        Assert.Equal("refresh", result.RefreshToken);
        Assert.Equal((uint)0, user.FailedLoginCount);
        UserSessionRepositoryMock.Verify(x => x.AddAsync(
            // Sessions from before the change carry version 2 and stop refreshing; the new one has 3.
            It.Is<ATMS.Admin.Data.Entities.Tokens.UserSession>(session => session.UserId == user.Id && session.SessionVersion == 3), It.IsAny<CancellationToken>()), Times.Once);
        UserRepositoryMock.Verify(x => x.TrySavePasswordChangeAsync(user, 2, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_IncorrectCurrentPassword_ReturnsFieldErrorAndCountsAttempt()
    {
        UserRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { PasswordHash = "old-hash" });

        var error = await Assert.ThrowsAsync<ValidationException>(() => CreateHandler().Handle(Command(), CancellationToken.None));

        Assert.Contains(error.Errors, failure => failure.PropertyName == "OldPassword");
        UserRepositoryMock.Verify(x => x.RegisterFailedPasswordAsync(
            It.IsAny<Guid>(), 5, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        UserSessionRepositoryMock.Verify(x => x.AddAsync(
            It.IsAny<ATMS.Admin.Data.Entities.Tokens.UserSession>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CurrentUserNotFound_ThrowsNotFound()
    {
        UserRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<EntityException>(() => CreateHandler().Handle(Command(), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_CombinedSaveFails_DoesNotReturnReplacementTokens()
    {
        var user = new User { Id = Guid.NewGuid(), PasswordHash = "old-hash" };
        UserRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        PasswordHasherServiceMock.Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        AccessTokenServiceMock.Setup(x => x.GenerateTokenAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccessTokenResult("access", DateTime.UtcNow.AddMinutes(10)));
        RefreshTokenServiceMock.Setup(x => x.GenerateTokenAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RefreshTokenResult("refresh", "hash", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(7)));
        UserRepositoryMock.Setup(x => x.TrySavePasswordChangeAsync(It.IsAny<User>(), It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException());

        await Assert.ThrowsAsync<InvalidOperationException>(() => CreateHandler().Handle(Command(), CancellationToken.None));

        UserSessionRepositoryMock.Verify(x => x.AddAsync(
            It.IsAny<ATMS.Admin.Data.Entities.Tokens.UserSession>(), It.IsAny<CancellationToken>()), Times.Once);
        UserRepositoryMock.Verify(x => x.TrySavePasswordChangeAsync(It.IsAny<User>(), It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ReplacementSessionCannotBePrepared_DoesNotSavePassword()
    {
        var user = new User { Id = Guid.NewGuid(), PasswordHash = "old-hash" };
        UserRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        PasswordHasherServiceMock.Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        AccessTokenServiceMock.Setup(x => x.GenerateTokenAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccessTokenResult("access", DateTime.UtcNow.AddMinutes(10)));
        RefreshTokenServiceMock.Setup(x => x.GenerateTokenAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RefreshTokenResult("refresh", "hash", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(7)));
        UserSessionRepositoryMock.Setup(x => x.AddAsync(
                It.IsAny<ATMS.Admin.Data.Entities.Tokens.UserSession>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException());

        await Assert.ThrowsAsync<InvalidOperationException>(() => CreateHandler().Handle(Command(), CancellationToken.None));

        UserRepositoryMock.Verify(x => x.TrySavePasswordChangeAsync(It.IsAny<User>(), It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // The attempt that locks the account reports the lock at once (423), not on the next try.
    [Fact]
    public async Task Handle_AttemptThatLocksTheAccount_ReportsTheLock()
    {
        var user = new User { Id = Guid.NewGuid(), PasswordHash = "old-hash", UserStatusId = (int)UserStatusEnum.Active };
        UserRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        UserRepositoryMock.Setup(x => x.RegisterFailedPasswordAsync(
                user.Id, 5, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var error = await Assert.ThrowsAsync<AuthException>(() => CreateHandler().Handle(Command(), CancellationToken.None));

        Assert.Equal(AuthErrorTypeEnum.AccountLocked, error.AuthErrorType);
        Assert.StartsWith(AuthMessages.AccountLocked.Split('{')[0], error.Message);
    }

    [Fact]
    public async Task Handle_WhileTimedLockoutRuns_RefusesWithoutCheckingThePassword()
    {
        var user = new User
        {
            Id = Guid.NewGuid(), PasswordHash = "old-hash",
            UserStatusId = (int)UserStatusEnum.Locked, LockoutEnd = DateTime.UtcNow.AddMinutes(10)
        };
        UserRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var error = await Assert.ThrowsAsync<AuthException>(() => CreateHandler().Handle(Command(), CancellationToken.None));

        Assert.Equal(AuthErrorTypeEnum.AccountLocked, error.AuthErrorType);
        PasswordHasherServiceMock.Verify(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    // Two password changes that read the same version: the one that commits second finds it moved and
    // gives up, so only one new session exists under the new version.
    [Fact]
    public async Task Handle_ConcurrentChangeWonTheRace_RefusesWithConflict()
    {
        var user = new User { Id = Guid.NewGuid(), PasswordHash = "old-hash", UserStatusId = (int)UserStatusEnum.Active };
        UserRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        PasswordHasherServiceMock.Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        AccessTokenServiceMock.Setup(x => x.GenerateTokenAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccessTokenResult("access", DateTime.UtcNow.AddMinutes(10)));
        RefreshTokenServiceMock.Setup(x => x.GenerateTokenAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RefreshTokenResult("refresh", "hash", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(7)));
        UserRepositoryMock.Setup(x => x.TrySavePasswordChangeAsync(It.IsAny<User>(), It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var error = await Assert.ThrowsAsync<ATMS.Application.Exceptions.Conflict.ConflictException>(
            () => CreateHandler().Handle(Command(), CancellationToken.None));

        Assert.Equal(AccountMessages.PasswordChangedConcurrently, error.Message);
    }
}

using System.Linq.Expressions;
using ATMS.Admin.Contracts.Commands.Authentication;
using ATMS.Admin.Contracts.Enums;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Service.Handlers.Authentication;
using ATMS.Admin.Service.Security.Models;
using ATMS.Application.Exceptions.Auth;
using Moq;

namespace Admin.Services.Tests.Handlers.Authentication;

public class LoginHandlerTest : BaseHandlerTest
{
    private readonly LoginHandler _handler;
 
    private const string ValidPassword = "ValidPass1!";
    private const string FakeAccessToken = "fake-access-token";
    private const string FakeRefreshToken = "fake-refresh-token";
 
    public LoginHandlerTest()
    {
        _handler = new LoginHandler(
            UserRepositoryMock.Object,
            AccessTokenServiceMock.Object,
            RefreshTokenServiceMock.Object,
            PasswordHasherServiceMock.Object);
    }
 
    private User CreateUser(uint failedLoginCount = 0, int? statusId = null, bool isEmailConfirmed = true) =>
        new()
        {
            Id = Guid.NewGuid(),
            Email = Faker.Internet.Email(),
            PasswordHash = Faker.Random.AlphaNumeric(32),
            FailedLoginCount = failedLoginCount,
            EmailConfirmed = isEmailConfirmed,
            UserStatusId = statusId ?? (int)UserStatus.Active
        };
 
    private LoginCommand CreateCommand(string? email = null, string? password = null) =>
        new()
        {
            Email = email ?? Faker.Internet.Email(),
            Password = password ?? ValidPassword
        };
 
    private void SetupUser(User user) =>
        UserRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
 
    private void SetupPasswordMatch(bool match) =>
        PasswordHasherServiceMock
            .Setup(p => p.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(match);
 
    private void SetupTokenServices(User user)
    {
        AccessTokenServiceMock
            .Setup(s => s.GenerateTokenAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccessTokenResult(FakeAccessToken, DateTime.UtcNow.AddMinutes(60)));
 
        RefreshTokenServiceMock
            .Setup(s => s.GenerateTokenAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(FakeRefreshToken);
    }
 
 
    [Fact]
    public async Task Handle_WithValidCredentials_ReturnsAccessInfo()
    {
        var user = CreateUser();
        var command = CreateCommand();
 
        SetupUser(user);
        SetupPasswordMatch(true);
        SetupTokenServices(user);
 
        var result = await _handler.Handle(command, CancellationToken.None);
 
        Assert.Equal(FakeAccessToken, result.AccessToken);
        Assert.Equal(FakeRefreshToken, result.RefreshToken);
    }
 
    [Fact]
    public async Task Handle_WithValidCredentials_ResetsFailedLoginCount()
    {
        var user = CreateUser(failedLoginCount: 3);
        var command = CreateCommand();
 
        SetupUser(user);
        SetupPasswordMatch(true);
        SetupTokenServices(user);
 
        await _handler.Handle(command, CancellationToken.None);
 
        Assert.Equal((uint)0, user.FailedLoginCount);
    }
 
 
    [Fact]
    public async Task Handle_WithWrongPassword_ThrowsAuthException()
    {
        var user = CreateUser();
        var command = CreateCommand();
 
        SetupUser(user);
        SetupPasswordMatch(false);
 
        var exception = await Assert.ThrowsAsync<AuthException>(() =>
            _handler.Handle(command, CancellationToken.None));
 
        Assert.Equal(AuthErrorType.InvalidCredentials, exception.AuthErrorType);
    }
 
    [Fact]
    public async Task Handle_WithWrongPassword_IncrementsFailedLoginCount()
    {
        var user = CreateUser(failedLoginCount: 2);
        var command = CreateCommand();
 
        SetupUser(user);
        SetupPasswordMatch(false);
 
        await Assert.ThrowsAsync<AuthException>(() =>
            _handler.Handle(command, CancellationToken.None));
 
        Assert.Equal((uint)3, user.FailedLoginCount);
    }
 
    [Fact]
    public async Task Handle_WhenFailedLoginCountReachesFive_LocksUser()
    {
        var user = CreateUser(failedLoginCount: 4);
        var command = CreateCommand();
 
        SetupUser(user);
        SetupPasswordMatch(false);
 
        await Assert.ThrowsAsync<AuthException>(() =>
            _handler.Handle(command, CancellationToken.None));
 
        Assert.Equal((int)UserStatus.Locked, user.UserStatusId);
        Assert.Equal((uint)0, user.FailedLoginCount);
        Assert.True(user.LockoutEnd.HasValue);
        Assert.True(user.LockoutEnd.Value > DateTime.UtcNow);
    }
    
    [Fact]
    public async Task Handle_WhenUserNotFound_ThrowsAuthException()
    {
        UserRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = CreateCommand();
        var exception = await Assert.ThrowsAsync<AuthException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Equal(AuthErrorType.InvalidCredentials, exception.AuthErrorType);
    }

    [Fact]
    public async Task Handle_WhenEmailNotConfirmed_ThrowsAuthException()
    {
        var user = CreateUser(isEmailConfirmed: false);
        var command = CreateCommand();

        SetupUser(user);

        var exception = await Assert.ThrowsAsync<AuthException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Equal(AuthErrorType.EmailNotConfirmed, exception.AuthErrorType);
    }

    [Fact]
    public async Task Handle_WhenAccountIsInactive_ThrowsAuthException()
    {
        var user = CreateUser(statusId: (int)UserStatus.Inactive);
        var command = CreateCommand();

        SetupUser(user);

        var exception = await Assert.ThrowsAsync<AuthException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Equal(AuthErrorType.AccountInactive, exception.AuthErrorType);
    }

    [Fact]
    public async Task Handle_WhenAccountIsLockedAndLockoutNotExpired_ThrowsAuthException()
    {
        var user = CreateUser(statusId: (int)UserStatus.Locked);
        user.LockoutEnd = DateTime.UtcNow.AddMinutes(10);
        var command = CreateCommand();

        SetupUser(user);

        var exception = await Assert.ThrowsAsync<AuthException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Equal(AuthErrorType.AccountLocked, exception.AuthErrorType);
    }

    [Fact]
    public async Task Handle_WhenAccountIsLockedButLockoutExpired_AllowsLogin()
    {
        var user = CreateUser(statusId: (int)UserStatus.Locked);
        user.LockoutEnd = DateTime.UtcNow.AddMinutes(-1); // срок истёк
        var command = CreateCommand();

        SetupUser(user);
        SetupPasswordMatch(true);
        SetupTokenServices(user);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(FakeAccessToken, result.AccessToken);
        Assert.Equal((int)UserStatus.Active, user.UserStatusId);
        Assert.Null(user.LockoutEnd);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_CallsSaveAsync()
    {
        var user = CreateUser();
        var command = CreateCommand();

        SetupUser(user);
        SetupPasswordMatch(true);
        SetupTokenServices(user);

        await _handler.Handle(command, CancellationToken.None);

        UserRepositoryMock.Verify(
            r => r.SaveAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithWrongPassword_DoesNotCallSaveAsync()
    {
        var user = CreateUser();
        var command = CreateCommand();

        SetupUser(user);
        SetupPasswordMatch(false);

        await Assert.ThrowsAsync<AuthException>(() =>
            _handler.Handle(command, CancellationToken.None));

        UserRepositoryMock.Verify(
            r => r.SaveAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task Handle_WithWrongPassword_BeforeThreshold_DoesNotLockUser(int failedCount)
    {
        var user = CreateUser(failedLoginCount: (uint)failedCount);
        var command = CreateCommand();

        SetupUser(user);
        SetupPasswordMatch(false);

        await Assert.ThrowsAsync<AuthException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Equal((int)UserStatus.Active, user.UserStatusId);
        Assert.Null(user.LockoutEnd);
    }
}

using System.Linq.Expressions;
using ATMS.Admin.Contracts.Commands.Authentication;
using ATMS.Admin.Contracts.Enums;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Service.Exceptions.Auth;
using ATMS.Admin.Service.Handlers.Authentication;
using ATMS.Admin.Service.Security.Models;
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
 
    private User CreateUser(uint failedLoginCount = 0, int? statusId = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            Email = Faker.Internet.Email(),
            PasswordHash = Faker.Random.AlphaNumeric(32),
            FailedLoginCount = failedLoginCount,
            UserStatusId = statusId ?? (int)UserStatusEnum.Active
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
 
        Assert.Equal(AuthErrorType.PasswordMismatch, exception.AuthErrorType);
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
 
        Assert.Equal((int)UserStatusEnum.Locked, user.UserStatusId);
        Assert.Equal((uint)0, user.FailedLoginCount);
        Assert.True(user.LockoutEnd.HasValue);
        Assert.True(user.LockoutEnd.Value > DateTime.UtcNow);
    }
}

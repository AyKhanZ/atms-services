using System.Linq.Expressions;
using ATMS.Admin.Contracts.Commands.Authentication;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Entities.Tokens;
using ATMS.Admin.Service.Handlers.Authentication;
using ATMS.Admin.Service.Security.Models;
using ATMS.Application.Exceptions.Auth;
using ATMS.Application.Exceptions.Enums;
using ATMS.Data.Enums;
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
            UserSessionRepositoryMock.Object,
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
            .Setup(s => s.GenerateTokenAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RefreshTokenResult(
                FakeRefreshToken,
                "refresh-token-hash",
                DateTime.UtcNow.AddDays(7),
                DateTime.UtcNow.AddDays(90)));
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
    public async Task Handle_WhenUserLogsInTwice_CreatesIndependentSessionFamilies()
    {
        var user = CreateUser();
        var sessions = new List<UserSession>();
        SetupUser(user);
        SetupPasswordMatch(true);
        SetupTokenServices(user);
        UserSessionRepositoryMock
            .Setup(repository => repository.AddAsync(
                It.IsAny<UserSession>(),
                It.IsAny<CancellationToken>()))
            .Callback<UserSession, CancellationToken>((session, _) => sessions.Add(session));

        await _handler.Handle(CreateCommand(), CancellationToken.None);
        await _handler.Handle(CreateCommand(), CancellationToken.None);

        Assert.Equal(2, sessions.Count);
        Assert.NotEqual(sessions[0].FamilyId, sessions[1].FamilyId);
        Assert.All(sessions, session => Assert.Equal(user.Id, session.UserId));
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
 
        Assert.Equal(AuthErrorTypeEnum.InvalidCredentials, exception.AuthErrorType);
    }
 
    // The count is updated in the database in one statement, so parallel wrong passwords cannot all
    // read the same number; the handler only asks for it.
    [Fact]
    public async Task Handle_WithWrongPassword_CountsTheAttemptAtomically()
    {
        var user = CreateUser(failedLoginCount: 4);
        SetupUser(user);
        SetupPasswordMatch(false);

        await Assert.ThrowsAsync<AuthException>(() =>
            _handler.Handle(CreateCommand(), CancellationToken.None));

        UserRepositoryMock.Verify(x => x.RegisterFailedPasswordAsync(
            user.Id,
            5,
            It.IsAny<DateTime>(),
            It.Is<DateTime>(end => end > DateTime.UtcNow.AddMinutes(14) && end <= DateTime.UtcNow.AddMinutes(15)),
            It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal((uint)4, user.FailedLoginCount);
    }

    // A session remembers the version it was issued under, so a later password change ends it.
    [Fact]
    public async Task Handle_WithValidCredentials_IssuesSessionUnderCurrentVersion()
    {
        var user = CreateUser();
        user.SessionVersion = 3;
        SetupUser(user);
        SetupPasswordMatch(true);
        SetupTokenServices(user);

        await _handler.Handle(CreateCommand(), CancellationToken.None);

        UserSessionRepositoryMock.Verify(x => x.AddAsync(
            It.Is<ATMS.Admin.Data.Entities.Tokens.UserSession>(session => session.SessionVersion == 3),
            It.IsAny<CancellationToken>()), Times.Once);
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

        Assert.Equal(AuthErrorTypeEnum.InvalidCredentials, exception.AuthErrorType);
    }

    [Fact]
    public async Task Handle_WhenEmailNotConfirmed_ThrowsAuthException()
    {
        var user = CreateUser(isEmailConfirmed: false);
        var command = CreateCommand();

        SetupUser(user);

        var exception = await Assert.ThrowsAsync<AuthException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Equal(AuthErrorTypeEnum.EmailNotConfirmed, exception.AuthErrorType);
    }

    [Fact]
    public async Task Handle_WhenAccountIsInactive_ThrowsAuthException()
    {
        var user = CreateUser(statusId: (int)UserStatusEnum.Inactive);
        var command = CreateCommand();

        SetupUser(user);

        var exception = await Assert.ThrowsAsync<AuthException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Equal(AuthErrorTypeEnum.AccountInactive, exception.AuthErrorType);
    }

    [Fact]
    public async Task Handle_WhenAccountIsLockedAndLockoutNotExpired_ThrowsAuthException()
    {
        var user = CreateUser(statusId: (int)UserStatusEnum.Locked);
        user.LockoutEnd = DateTime.UtcNow.AddMinutes(10);
        var command = CreateCommand();

        SetupUser(user);

        var exception = await Assert.ThrowsAsync<AuthException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Equal(AuthErrorTypeEnum.AccountLocked, exception.AuthErrorType);
    }

    // Locked with no end date was set by an administrator; the right password does not lift it.
    [Fact]
    public async Task Handle_WhenAccountIsLockedByAdministrator_RefusesEvenWithRightPassword()
    {
        var user = CreateUser(statusId: (int)UserStatusEnum.Locked);
        user.LockoutEnd = null;
        SetupUser(user);
        SetupPasswordMatch(true);

        var exception = await Assert.ThrowsAsync<AuthException>(() =>
            _handler.Handle(CreateCommand(), CancellationToken.None));

        Assert.Equal(AuthErrorTypeEnum.AccountLocked, exception.AuthErrorType);
        Assert.Equal((int)UserStatusEnum.Locked, user.UserStatusId);
    }

    [Fact]
    public async Task Handle_WhenAccountIsLockedButLockoutExpired_AllowsLogin()
    {
        var user = CreateUser(statusId: (int)UserStatusEnum.Locked);
        user.LockoutEnd = DateTime.UtcNow.AddMinutes(-1); // срок истёк
        var command = CreateCommand();

        SetupUser(user);
        SetupPasswordMatch(true);
        SetupTokenServices(user);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(FakeAccessToken, result.AccessToken);
        Assert.Equal((int)UserStatusEnum.Active, user.UserStatusId);
        Assert.Null(user.LockoutEnd);
    }

    [Fact]
    public async Task Handle_WrongPasswordAfterExpiredLockout_CanLockAgain()
    {
        var user = CreateUser(failedLoginCount: 4, statusId: (int)UserStatusEnum.Locked);
        user.LockoutEnd = DateTime.UtcNow.AddMinutes(-1);
        SetupUser(user);
        SetupPasswordMatch(false);

        await Assert.ThrowsAsync<AuthException>(() => _handler.Handle(CreateCommand(), CancellationToken.None));

        // The finished lockout is lifted and saved first, then the attempt is counted in the
        // database, where reaching five locks the account again.
        Assert.Equal((int)UserStatusEnum.Active, user.UserStatusId);
        UserRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
        UserRepositoryMock.Verify(x => x.RegisterFailedPasswordAsync(
            user.Id, 5, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
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
    public async Task Handle_WithWrongPassword_PersistsFailedAttempt()
    {
        var user = CreateUser();
        var command = CreateCommand();

        SetupUser(user);
        SetupPasswordMatch(false);

        await Assert.ThrowsAsync<AuthException>(() =>
            _handler.Handle(command, CancellationToken.None));

        UserRepositoryMock.Verify(
            r => r.SaveAsync(It.IsAny<CancellationToken>()),
            Times.Once);
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

        Assert.Equal((int)UserStatusEnum.Active, user.UserStatusId);
        Assert.Null(user.LockoutEnd);
    }
}

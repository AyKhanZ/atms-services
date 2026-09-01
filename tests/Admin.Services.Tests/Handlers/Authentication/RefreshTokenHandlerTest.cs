using ATMS.Admin.Contracts.Commands.Authentication;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Entities.Tokens;
using ATMS.Admin.Service.Handlers.Authentication;
using ATMS.Admin.Service.Security.Models;
using ATMS.Application.Exceptions.Auth;
using ATMS.Data.Enums;
using Moq;

namespace Admin.Services.Tests.Handlers.Authentication;

public class RefreshTokenHandlerTest : BaseHandlerTest
{
    private const string RefreshToken = "refresh-token";
    private const string NewRefreshToken = "new-refresh-token";
    private const string TokenHash = "token-hash";

    private readonly RefreshTokenHandler _handler;

    public RefreshTokenHandlerTest()
    {
        _handler = new RefreshTokenHandler(
            AccessTokenServiceMock.Object,
            RefreshTokenServiceMock.Object,
            UserSessionRepositoryMock.Object);

        RefreshTokenServiceMock
            .Setup(service => service.HashToken(RefreshToken))
            .Returns(TokenHash);
    }

    [Fact]
    public async Task Handle_WhenSessionIsActive_RotatesOnlyThatSession()
    {
        var session = CreateSession();
        SetupSuccessfulRotation(session);

        var result = await _handler.Handle(CreateCommand(), CancellationToken.None);

        Assert.Equal(NewRefreshToken, result.RefreshToken);
        UserSessionRepositoryMock.Verify(repository => repository.RotateAsync(
            session,
            It.Is<UserSession>(replacement =>
                replacement.UserId == session.UserId
                && replacement.FamilyId == session.FamilyId
                && replacement.TokenHash == "new-token-hash"),
            It.IsAny<DateTime>(),
            It.IsAny<CancellationToken>()), Times.Once);
        UserSessionRepositoryMock.Verify(repository => repository.RevokeAllAsync(
            It.IsAny<Guid>(),
            It.IsAny<DateTime>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenSessionDoesNotExist_ThrowsInvalidToken()
    {
        UserSessionRepositoryMock
            .Setup(repository => repository.FindByTokenHashAsync(
                TokenHash,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserSession?)null);

        var exception = await Assert.ThrowsAsync<AuthException>(
            () => _handler.Handle(CreateCommand(), CancellationToken.None));

        Assert.Equal(AuthErrorType.InvalidToken, exception.AuthErrorType);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task Handle_WhenSessionOrFamilyExpired_RevokesFamily(
        bool sessionExpired,
        bool familyExpired)
    {
        var session = CreateSession();
        session.ExpiresAt = sessionExpired
            ? DateTime.UtcNow.AddMinutes(-1)
            : DateTime.UtcNow.AddDays(1);
        session.FamilyExpiresAt = familyExpired
            ? DateTime.UtcNow.AddMinutes(-1)
            : DateTime.UtcNow.AddDays(30);

        SetupSession(session);

        await Assert.ThrowsAsync<AuthException>(
            () => _handler.Handle(CreateCommand(), CancellationToken.None));

        VerifyFamilyRevoked(session.FamilyId);
    }

    [Fact]
    public async Task Handle_WhenTokenWasAlreadyUsed_RevokesFamily()
    {
        var session = CreateSession();
        session.RevokedAt = DateTime.UtcNow.AddSeconds(-1);
        SetupSession(session);

        await Assert.ThrowsAsync<AuthException>(
            () => _handler.Handle(CreateCommand(), CancellationToken.None));

        VerifyFamilyRevoked(session.FamilyId);
    }

    [Fact]
    public async Task Handle_WhenUserIsInactive_RevokesFamilyAndThrowsForbidden()
    {
        var session = CreateSession();
        session.User.UserStatusId = (int)UserStatusEnum.Inactive;
        SetupSession(session);

        var exception = await Assert.ThrowsAsync<AuthException>(
            () => _handler.Handle(CreateCommand(), CancellationToken.None));

        Assert.Equal(AuthErrorType.AccountInactive, exception.AuthErrorType);
        VerifyFamilyRevoked(session.FamilyId);
    }

    [Fact]
    public async Task Handle_WhenRotationLosesConcurrencyRace_RevokesFamily()
    {
        var session = CreateSession();
        SetupSuccessfulRotation(session, rotationSucceeded: false);

        var exception = await Assert.ThrowsAsync<AuthException>(
            () => _handler.Handle(CreateCommand(), CancellationToken.None));

        Assert.Equal(AuthErrorType.InvalidToken, exception.AuthErrorType);
        VerifyFamilyRevoked(session.FamilyId);
    }

    private static RefreshTokenCommand CreateCommand()
    {
        return new RefreshTokenCommand { RefreshToken = RefreshToken };
    }

    private static UserSession CreateSession()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            UserStatusId = (int)UserStatusEnum.Active
        };

        return new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            FamilyId = Guid.NewGuid(),
            TokenHash = TokenHash,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            FamilyExpiresAt = DateTime.UtcNow.AddDays(30)
        };
    }

    private void SetupSession(UserSession session)
    {
        UserSessionRepositoryMock
            .Setup(repository => repository.FindByTokenHashAsync(
                TokenHash,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
    }

    private void SetupSuccessfulRotation(UserSession session, bool rotationSucceeded = true)
    {
        SetupSession(session);

        AccessTokenServiceMock
            .Setup(service => service.GenerateTokenAsync(session.User, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccessTokenResult("access-token", DateTime.UtcNow.AddMinutes(25)));

        RefreshTokenServiceMock
            .Setup(service => service.GenerateTokenAsync(
                session.FamilyExpiresAt,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RefreshTokenResult(
                NewRefreshToken,
                "new-token-hash",
                DateTime.UtcNow.AddDays(7),
                session.FamilyExpiresAt));

        UserSessionRepositoryMock
            .Setup(repository => repository.RotateAsync(
                session,
                It.IsAny<UserSession>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(rotationSucceeded);
    }

    private void VerifyFamilyRevoked(Guid familyId)
    {
        UserSessionRepositoryMock.Verify(repository => repository.RevokeFamilyAsync(
            familyId,
            It.IsAny<DateTime>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}

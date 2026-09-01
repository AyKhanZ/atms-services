using ATMS.Admin.Contracts.Commands.Authentication;
using ATMS.Admin.Data.Entities.Tokens;
using ATMS.Admin.Service.Handlers.Authentication;
using Moq;

namespace Admin.Services.Tests.Handlers.Authentication;

public class LogoutHandlerTest : BaseHandlerTest
{
    private const string RefreshToken = "refresh-token";
    private const string TokenHash = "token-hash";

    private readonly LogoutHandler _handler;

    public LogoutHandlerTest()
    {
        _handler = new LogoutHandler(
            UserSessionRepositoryMock.Object,
            RefreshTokenServiceMock.Object);

        RefreshTokenServiceMock
            .Setup(service => service.HashToken(RefreshToken))
            .Returns(TokenHash);
    }

    [Fact]
    public async Task Handle_WhenSessionIsActive_RevokesOnlyPresentedSession()
    {
        var session = CreateSession();
        UserSessionRepositoryMock
            .Setup(repository => repository.FindByTokenHashAsync(
                TokenHash,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        await _handler.Handle(CreateCommand(), CancellationToken.None);

        UserSessionRepositoryMock.Verify(repository => repository.RevokeAsync(
            session,
            It.IsAny<DateTime>(),
            It.IsAny<CancellationToken>()), Times.Once);
        UserSessionRepositoryMock.Verify(repository => repository.RevokeAllAsync(
            It.IsAny<Guid>(),
            It.IsAny<DateTime>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Handle_WhenSessionIsMissingOrAlreadyRevoked_IsIdempotent(bool revoked)
    {
        UserSession? session = revoked ? CreateSession() : null;
        if (session is not null)
        {
            session.RevokedAt = DateTime.UtcNow;
        }

        UserSessionRepositoryMock
            .Setup(repository => repository.FindByTokenHashAsync(
                TokenHash,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        await _handler.Handle(CreateCommand(), CancellationToken.None);

        UserSessionRepositoryMock.Verify(repository => repository.RevokeAsync(
            It.IsAny<UserSession>(),
            It.IsAny<DateTime>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    private static LogoutCommand CreateCommand()
    {
        return new LogoutCommand { RefreshToken = RefreshToken };
    }

    private static UserSession CreateSession()
    {
        return new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            FamilyId = Guid.NewGuid(),
            TokenHash = TokenHash,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            FamilyExpiresAt = DateTime.UtcNow.AddDays(90)
        };
    }
}

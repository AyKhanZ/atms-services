using System.Data.Common;
using Admin.Services.Tests.Fixtures;
using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Data.DbContexts;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Entities.Tokens;
using ATMS.Admin.Data.Repositories;
using ATMS.Admin.Service.Handlers.Account;
using ATMS.Admin.Service.Security;
using ATMS.Application.Exceptions.Auth;
using ATMS.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Admin.Services.Tests.Handlers.Account;

public sealed class ResetPasswordHandlerIntegrationTest(AdminPostgreSqlFixture postgres)
    : IClassFixture<AdminPostgreSqlFixture>
{
    private readonly PasswordHasherService hasher = new();
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(15);

    [Fact]
    public async Task Handle_WhenTokenConsumedBeforeUserRead_RejectsWithoutChangingNewCredentials()
    {
        var (userId, token) = await SeedResetAsync();
        var pause = new PauseUserReadInterceptor();
        await using var delayedContext = postgres.CreateContext(pause);
        var delayedReset = CreateHandler(delayedContext).Handle(Command(token, "DelayedPassword1!"), CancellationToken.None);
        await pause.UserReadStarted.WaitAsync(Timeout);

        string freshToken;
        try
        {
            await using var winningContext = postgres.CreateContext();
            await CreateHandler(winningContext).Handle(Command(token, "WinningPassword1!"), CancellationToken.None);

            // A new sign-in and reset link issued after the winner must survive the stale request.
            await using var nextContext = postgres.CreateContext();
            var nextToken = CreateToken(userId);
            freshToken = nextToken.Token;
            nextContext.PasswordResetTokens.Add(nextToken);
            nextContext.UserSessions.Add(CreateSession(userId, 1));
            await nextContext.SaveChangesAsync();
        }
        finally
        {
            pause.Resume();
        }

        var error = await Assert.ThrowsAsync<AuthException>(() => delayedReset.WaitAsync(Timeout));
        Assert.Equal(AuthErrorType.InvalidToken, error.AuthErrorType);

        await using var verification = postgres.CreateContext();
        var user = await verification.Users.SingleAsync(user => user.Id == userId);
        Assert.True(hasher.Verify("WinningPassword1!", user.PasswordHash));
        Assert.Equal(1, user.SessionVersion);
        var remainingToken = Assert.Single(await verification.PasswordResetTokens
            .Where(reset => reset.UserId == userId).ToListAsync());
        Assert.Equal(freshToken, remainingToken.Token);
        var sessions = await verification.UserSessions.Where(session => session.UserId == userId).ToListAsync();
        Assert.NotNull(Assert.Single(sessions, session => session.SessionVersion == 0).RevokedAt);
        Assert.Null(Assert.Single(sessions, session => session.SessionVersion == 1).RevokedAt);
    }

    [Fact]
    public async Task Handle_WhenSaveFails_RollsBackPasswordVersionTokensAndSessions()
    {
        var (userId, token) = await SeedResetAsync();
        await using var failingContext = postgres.CreateContext(new FailAfterSaveInterceptor());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateHandler(failingContext).Handle(Command(token, "NewPassword1!"), CancellationToken.None));

        await using var verification = postgres.CreateContext();
        var user = await verification.Users.SingleAsync(user => user.Id == userId);
        Assert.True(hasher.Verify("OldPassword1!", user.PasswordHash));
        Assert.Equal(0, user.SessionVersion);
        Assert.Equal((int)UserStatusEnum.Locked, user.UserStatusId);
        Assert.NotNull(user.LockoutEnd);
        Assert.Equal((uint)3, user.FailedLoginCount);
        Assert.Equal(2, await verification.PasswordResetTokens.CountAsync(reset => reset.UserId == userId));
        Assert.True(await verification.PasswordResetTokens.AnyAsync(reset => reset.Token == token));
        Assert.Null((await verification.UserSessions.SingleAsync(session => session.UserId == userId)).RevokedAt);
    }

    [Fact]
    public async Task Handle_WhenTokenValid_CommitsPasswordVersionTokenConsumptionAndSessionRevocation()
    {
        var (userId, token) = await SeedResetAsync();
        await using var context = postgres.CreateContext();

        await CreateHandler(context).Handle(Command(token, "NewPassword1!"), CancellationToken.None);

        await using var verification = postgres.CreateContext();
        var user = await verification.Users.SingleAsync(user => user.Id == userId);
        Assert.True(hasher.Verify("NewPassword1!", user.PasswordHash));
        Assert.Equal(1, user.SessionVersion);
        Assert.Equal((int)UserStatusEnum.Active, user.UserStatusId);
        Assert.Null(user.LockoutEnd);
        Assert.Equal((uint)0, user.FailedLoginCount);
        Assert.False(await verification.PasswordResetTokens.AnyAsync(reset => reset.UserId == userId));
        Assert.NotNull((await verification.UserSessions.SingleAsync(session => session.UserId == userId)).RevokedAt);
    }

    private ResetPasswordHandler CreateHandler(AdminDbContext context) => new(
        new PasswordResetTokenRepository(context), new UserRepository(context), hasher);

    private ResetPasswordCommand Command(string token, string password) => new()
    {
        Token = token,
        Password = password,
        ConfirmPassword = password
    };

    private async Task<(Guid UserId, string Token)> SeedResetAsync()
    {
        await using var context = postgres.CreateContext();
        var email = $"{Guid.NewGuid():N}@example.test";
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            Name = "Test",
            Surname = "User",
            PasswordHash = hasher.Hash("OldPassword1!"),
            AvatarPath = "default-avatar.png",
            CreatedAt = DateTime.UtcNow,
            UserStatusId = (int)UserStatusEnum.Locked,
            LockoutEnd = DateTime.UtcNow.AddMinutes(10),
            FailedLoginCount = 3,
            SessionVersion = 0
        };
        var token = CreateToken(user.Id);
        context.Users.Add(user);
        context.PasswordResetTokens.AddRange(token, CreateToken(user.Id));
        context.UserSessions.Add(CreateSession(user.Id, 0));
        await context.SaveChangesAsync();
        return (user.Id, token.Token);
    }

    private PasswordResetToken CreateToken(Guid userId) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Token = Guid.NewGuid().ToString("N"),
        ExpiresAt = DateTime.UtcNow.AddMinutes(10)
    };

    private UserSession CreateSession(Guid userId, int version) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        FamilyId = Guid.NewGuid(),
        TokenHash = Guid.NewGuid().ToString("N"),
        CreatedAt = DateTime.UtcNow,
        ExpiresAt = DateTime.UtcNow.AddDays(1),
        FamilyExpiresAt = DateTime.UtcNow.AddDays(7),
        SessionVersion = version
    };

    private sealed class PauseUserReadInterceptor : DbCommandInterceptor
    {
        private readonly TaskCompletionSource userReadStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource resume = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int paused;

        public Task UserReadStarted => userReadStarted.Task;

        public void Resume() => resume.TrySetResult();

        public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            if (command.CommandText.Contains("FROM \"Users\"", StringComparison.Ordinal) &&
                Interlocked.Exchange(ref paused, 1) == 0)
            {
                userReadStarted.TrySetResult();
                await resume.Task.WaitAsync(Timeout, cancellationToken);
            }

            return result;
        }
    }

    private sealed class FailAfterSaveInterceptor : SaveChangesInterceptor
    {
        public override ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("Simulated failure before transaction commit.");
    }
}

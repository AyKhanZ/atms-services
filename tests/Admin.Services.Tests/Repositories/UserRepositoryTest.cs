using ATMS.Admin.Data.DbContexts;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Entities.Tokens;
using ATMS.Admin.Data.Repositories;
using ATMS.Data.Enums;
using Admin.Services.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace Admin.Services.Tests.Repositories;

// Runs the repository's raw SQL against a real Postgres in Docker, with every migration applied:
// row locks and conditional updates are what these methods rely on, and a mocked repository
// cannot show whether they hold under parallel requests.
public sealed class UserRepositoryTest(AdminPostgreSqlFixture postgres) : IClassFixture<AdminPostgreSqlFixture>
{
    // Ten wrong passwords at once: they are counted one by one, the fifth locks, and the ones after it
    // no longer count, so the next 15 minutes do not start with a used-up allowance.
    [Fact]
    public async Task RegisterFailedPassword_ParallelAttempts_LockOnceAndLeaveTheCountAtZero()
    {
        var userId = await CreateUserAsync((int)UserStatusEnum.Active, lockoutEnd: null);
        var now = DateTime.UtcNow;

        var results = await Task.WhenAll(Enumerable.Range(0, 10).Select(async _ =>
        {
            await using var context = CreateContext();
            return await new UserRepository(context)
                .RegisterFailedPasswordAsync(userId, 5, now, now.AddMinutes(15), CancellationToken.None);
        }));

        var user = await ReadUserAsync(userId);
        Assert.Equal((int)UserStatusEnum.Locked, user.UserStatusId);
        Assert.Equal((uint)0, user.FailedLoginCount);
        Assert.NotNull(user.LockoutEnd);
        Assert.Equal(4, results.Count(locked => !locked));
    }

    [Fact]
    public async Task RegisterFailedPassword_AfterTimedLockoutEnded_CountsFromZeroAgain()
    {
        var userId = await CreateUserAsync((int)UserStatusEnum.Locked, DateTime.UtcNow.AddMinutes(-1));
        await using var context = CreateContext();
        var now = DateTime.UtcNow;

        var locked = await new UserRepository(context)
            .RegisterFailedPasswordAsync(userId, 5, now, now.AddMinutes(15), CancellationToken.None);

        var user = await ReadUserAsync(userId);
        Assert.False(locked);
        Assert.Equal((uint)1, user.FailedLoginCount);
        Assert.Equal((int)UserStatusEnum.Active, user.UserStatusId);
        Assert.Null(user.LockoutEnd);
    }

    [Fact]
    public async Task RegisterFailedPassword_ManualLock_LeavesTheAccountUntouched()
    {
        var userId = await CreateUserAsync((int)UserStatusEnum.Locked, lockoutEnd: null);
        await using var context = CreateContext();
        var now = DateTime.UtcNow;

        var locked = await new UserRepository(context)
            .RegisterFailedPasswordAsync(userId, 5, now, now.AddMinutes(15), CancellationToken.None);

        var user = await ReadUserAsync(userId);
        Assert.True(locked);
        Assert.Equal((uint)0, user.FailedLoginCount);
        Assert.Null(user.LockoutEnd);
    }

    // Two password changes that read the same version: exactly one commits, the other rolls back
    // with nothing written, so one session at most is issued under the new version.
    [Fact]
    public async Task TrySavePasswordChange_TwoChangesFromTheSameVersion_OnlyOneWins()
    {
        var userId = await CreateUserAsync((int)UserStatusEnum.Active, lockoutEnd: null);
        await using var first = CreateContext();
        await using var second = CreateContext();
        var firstUser = await first.Users.SingleAsync(user => user.Id == userId);
        var secondUser = await second.Users.SingleAsync(user => user.Id == userId);
        firstUser.PasswordHash = "first";
        secondUser.PasswordHash = "second";
        first.UserSessions.Add(CreateReplacementSession(userId, "first"));
        second.UserSessions.Add(CreateReplacementSession(userId, "second"));

        var saved = await Task.WhenAll(
            new UserRepository(first).TrySavePasswordChangeAsync(firstUser, 0, DateTime.UtcNow, CancellationToken.None),
            new UserRepository(second).TrySavePasswordChangeAsync(secondUser, 0, DateTime.UtcNow, CancellationToken.None));

        var user = await ReadUserAsync(userId);
        Assert.Single(saved, ok => ok);
        Assert.Equal(1, user.SessionVersion);
        Assert.Equal(saved[0] ? "first" : "second", user.PasswordHash);
        await using var verification = CreateContext();
        var session = Assert.Single(await verification.UserSessions
            .Where(session => session.UserId == userId)
            .ToListAsync());
        Assert.Equal(saved[0] ? "first" : "second", session.TokenHash);
        Assert.Equal(user.SessionVersion, session.SessionVersion);
    }

    // Another device refreshes its token while the password changes: the rotated session is no
    // longer "active" when the change revokes sessions. That must not fail the change; every session
    // the device holds ends, and only the one issued with the new password stays.
    [Fact]
    public async Task TrySavePasswordChange_SessionRotatedMeanwhile_SucceedsAndKeepsOnlyTheNewSession()
    {
        var userId = await CreateUserAsync((int)UserStatusEnum.Active, lockoutEnd: null);
        await using (var setup = CreateContext())
        {
            setup.UserSessions.Add(CreateSession(userId, "device-old", version: 0));
            await setup.SaveChangesAsync();
        }

        await using var change = CreateContext();
        var user = await change.Users.SingleAsync(candidate => candidate.Id == userId);
        user.PasswordHash = "new";
        change.UserSessions.Add(CreateSession(userId, "changed", version: 1));

        await using (var device = CreateContext())
        {
            var old = await device.UserSessions.SingleAsync(session => session.TokenHash == "device-old");
            old.RevokedAt = DateTime.UtcNow;
            device.UserSessions.Add(CreateSession(userId, "device-rotated", version: 0));
            await device.SaveChangesAsync();
        }

        var saved = await new UserRepository(change)
            .TrySavePasswordChangeAsync(user, 0, DateTime.UtcNow, CancellationToken.None);

        await using var verification = CreateContext();
        var active = await verification.UserSessions
            .Where(session => session.UserId == userId && session.RevokedAt == null)
            .Select(session => session.TokenHash)
            .ToListAsync();
        Assert.True(saved);
        Assert.Equal(["changed"], active);
    }

    private UserSession CreateSession(Guid userId, string tokenHash, int version) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        FamilyId = Guid.NewGuid(),
        TokenHash = tokenHash,
        CreatedAt = DateTime.UtcNow,
        ExpiresAt = DateTime.UtcNow.AddDays(1),
        FamilyExpiresAt = DateTime.UtcNow.AddDays(7),
        SessionVersion = version
    };

    private AdminDbContext CreateContext() => postgres.CreateContext();

    private UserSession CreateReplacementSession(Guid userId, string tokenHash) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        FamilyId = Guid.NewGuid(),
        TokenHash = tokenHash,
        CreatedAt = DateTime.UtcNow,
        ExpiresAt = DateTime.UtcNow.AddDays(1),
        FamilyExpiresAt = DateTime.UtcNow.AddDays(7),
        SessionVersion = 1
    };

    private async Task<Guid> CreateUserAsync(int statusId, DateTime? lockoutEnd)
    {
        await using var context = CreateContext();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = $"{Guid.NewGuid():N}@example.test",
            NormalizedEmail = $"{Guid.NewGuid():N}@EXAMPLE.TEST",
            Name = "Test",
            Surname = "User",
            PasswordHash = "hash",
            AvatarPath = "default-avatar.png",
            CreatedAt = DateTime.UtcNow,
            UserStatusId = statusId,
            LockoutEnd = lockoutEnd
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user.Id;
    }

    private async Task<User> ReadUserAsync(Guid userId)
    {
        await using var context = CreateContext();
        return await context.Users.AsNoTracking().SingleAsync(user => user.Id == userId);
    }
}

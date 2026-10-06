using System.Data.Common;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Project.Services.Tests.Repositories;

public class UserRepositoryTest
{
    // Deleted users too, and by the normalised email: Admin would refuse to create either address again.
    [Fact]
    public async Task IsEmailTakenAsync_MatchesNormalizedEmailIncludingDeletedUsers()
    {
        var sql = await CaptureAsync(repository => repository.IsEmailTakenAsync("NIGAR@CLIENT.AZ", CancellationToken.None));

        Assert.Matches(@"""NormalizedEmail"" = @", sql);
        Assert.DoesNotMatch(@"UPPER|upper", sql);
        Assert.DoesNotMatch(@"""IsDeleted""", sql);
    }

    private static async Task<string> CaptureAsync(Func<UserRepository, Task> query)
    {
        var capture = new CaptureCommandInterceptor();
        var options = new DbContextOptionsBuilder<ProjectDbContext>()
            .UseNpgsql("Host=localhost;Database=query_capture_only")
            .AddInterceptors(new SuppressConnectionInterceptor(), capture)
            .Options;
        await using var context = new ProjectDbContext(options);

        await Assert.ThrowsAsync<QueryCompiledException>(() => query(new UserRepository(context)));

        return capture.Sql!;
    }

    private sealed class QueryCompiledException : Exception;

    private sealed class SuppressConnectionInterceptor : DbConnectionInterceptor
    {
        public override ValueTask<InterceptionResult> ConnectionOpeningAsync(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(InterceptionResult.Suppress());
        }
    }

    private sealed class CaptureCommandInterceptor : DbCommandInterceptor
    {
        public string? Sql { get; private set; }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            Sql = command.CommandText;
            throw new QueryCompiledException();
        }
    }
}

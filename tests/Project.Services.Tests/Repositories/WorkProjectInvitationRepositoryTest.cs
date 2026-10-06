using System.Data.Common;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Project.Services.Tests.Repositories;

public class WorkProjectInvitationRepositoryTest
{
    // An invitation nobody answered for 24 hours stops counting: hidden from the project, out of the
    // participant limit, and the email can be invited again.
    [Fact]
    public async Task GetLivePendingAsync_ReadsOnlyPendingInvitationsOfTheLast24Hours()
    {
        var sql = await CaptureAsync(repository => repository.GetLivePendingAsync(Guid.NewGuid(), CancellationToken.None));

        Assert.Matches(@"""Status"" = 1", sql);
        Assert.Matches(@"""CreatedAt"" > @", sql);
        Assert.Matches(@"""WorkProjectId"" = @", sql);
    }

    // A late answer from Admin must still settle an invitation older than 24 hours.
    [Fact]
    public async Task GetPendingByEmailAsync_ReadsPendingInvitationsOfAnyAge()
    {
        var sql = await CaptureAsync(repository => repository.GetPendingByEmailAsync("NIGAR@CLIENT.AZ", CancellationToken.None));

        Assert.Matches(@"""Status"" = 1", sql);
        Assert.Matches(@"""NormalizedEmail"" = @", sql);
        Assert.DoesNotMatch(@"""CreatedAt"" >", sql);
    }

    private static async Task<string> CaptureAsync(Func<WorkProjectInvitationRepository, Task> query)
    {
        var capture = new CaptureCommandInterceptor();
        var options = new DbContextOptionsBuilder<ProjectDbContext>()
            .UseNpgsql("Host=localhost;Database=query_capture_only")
            .AddInterceptors(new SuppressConnectionInterceptor(), capture)
            .Options;
        await using var context = new ProjectDbContext(options);

        await Assert.ThrowsAsync<QueryCompiledException>(() => query(new WorkProjectInvitationRepository(context)));

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

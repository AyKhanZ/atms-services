using System.Data.Common;
using ATMS.Data.Constants;
using ATMS.Project.Data.Criteria.WorkProjects;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Repositories;
using ATMS.Project.Services.Dashboard;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Project.Services.Tests.Realtime;

namespace Project.Services.Tests.Repositories;

public sealed class DashboardRepositoryTest
{
    private static readonly BusinessTimeZone Zone = new(TimeZoneInfo.FindSystemTimeZoneById("Asia/Baku"));

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetAsync_FirstAggregateTranslatesToPostgreSql(bool selectProject)
    {
        var options = new DbContextOptionsBuilder<ProjectDbContext>()
            .UseNpgsql("Host=localhost;Database=query_compilation_only")
            .AddInterceptors(new StopBeforeConnectionInterceptor())
            .Options;
        await using var context = new ProjectDbContext(options);
        var window = Zone.GetWindow(new DateTime(2026, 9, 25, 12, 0, 0, DateTimeKind.Utc), "30d", null, null);
        var repository = new DashboardRepository(context);

        var exception = await Record.ExceptionAsync(() => repository.GetAsync(
            new AccessibleWorkProjectsCriteria(Guid.NewGuid(), RoleIds.Employee),
            selectProject ? Guid.NewGuid() : null,
            window.Data,
            true,
            CancellationToken.None));

        Assert.IsType<QueryCompiledException>(exception);
    }

    // Grouping by hour, day and month translates differently; only a real PostgreSQL proves all three.
    // The queries only read, so any development database with the schema will do.
    [PostgresFact]
    public async Task GetAsync_EveryBucketSize_RunsOnPostgreSql()
    {
        var options = new DbContextOptionsBuilder<ProjectDbContext>()
            .UseNpgsql(Environment.GetEnvironmentVariable("ATMS_REALTIME_TEST_DB"))
            .Options;
        await using var context = new ProjectDbContext(options);
        var repository = new DashboardRepository(context);

        foreach (var period in new[] { "today", "30d", "12m" })
        {
            var window = Zone.GetWindow(DateTime.UtcNow, period, null, null);

            var data = await repository.GetAsync(
                new AccessibleWorkProjectsCriteria(Guid.NewGuid(), RoleIds.SuperAdmin),
                null,
                window.Data,
                true,
                CancellationToken.None);

            Assert.NotNull(data.CreatedByBucket);
            Assert.NotNull(data.DoneByBucket);
        }
    }

    private sealed class QueryCompiledException : Exception;

    private sealed class StopBeforeConnectionInterceptor : DbConnectionInterceptor
    {
        public override ValueTask<InterceptionResult> ConnectionOpeningAsync(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken = default) =>
            throw new QueryCompiledException();
    }
}

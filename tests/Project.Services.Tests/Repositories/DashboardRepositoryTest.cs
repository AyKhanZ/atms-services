using System.Data.Common;
using ATMS.Data.Constants;
using ATMS.Project.Data.Criteria.WorkProjects;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Repositories;
using ATMS.Project.Services.Dashboard;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Project.Services.Tests.Repositories;

public sealed class DashboardRepositoryTest
{
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
        var window = new BusinessTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Asia/Baku"))
            .GetWindow(new DateTime(2026, 9, 25, 12, 0, 0, DateTimeKind.Utc), 30);
        var repository = new DashboardRepository(context);

        var exception = await Record.ExceptionAsync(() => repository.GetAsync(
            new AccessibleWorkProjectsCriteria(Guid.NewGuid(), RoleIds.Employee),
            selectProject ? Guid.NewGuid() : null,
            window.TodayStartUtc,
            window.PeriodStartUtc,
            window.PreviousStartUtc,
            window.PeriodEndUtc,
            window.DueEndUtc,
            window.OffsetHours,
            true,
            CancellationToken.None));

        Assert.IsType<QueryCompiledException>(exception);
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

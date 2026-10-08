using System.Data.Common;
using ATMS.Data.Constants;
using ATMS.Project.Data.Criteria.WorkProjects;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Repositories;
using ATMS.Project.Services.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

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

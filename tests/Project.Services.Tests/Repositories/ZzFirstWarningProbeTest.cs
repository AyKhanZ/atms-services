using ATMS.Data.Constants;
using ATMS.Project.Data.Criteria.WorkProjects;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Repositories;
using ATMS.Project.Services.Dashboard;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Project.Services.Tests.Repositories;

// Temporary probe, read-only against the local database.
public sealed class ZzFirstWarningProbeTest
{
    private static ProjectDbContext Context() => new(new DbContextOptionsBuilder<ProjectDbContext>()
        .UseNpgsql(Environment.GetEnvironmentVariable("ATMS_REALTIME_TEST_DB"))
        .ConfigureWarnings(warnings => warnings.Throw(CoreEventId.FirstWithoutOrderByAndFilterWarning))
        .Options);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Dashboard(bool superAdmin)
    {
        await using var context = Context();
        var zone = new BusinessTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Asia/Baku"));
        var window = zone.GetWindow(DateTime.UtcNow, "30d", null, null);
        await new DashboardRepository(context).GetAsync(
            new AccessibleWorkProjectsCriteria(Guid.NewGuid(), superAdmin ? RoleIds.SuperAdmin : RoleIds.Employee),
            null, window.Data, true, CancellationToken.None);
    }

    [Fact]
    public async Task Progress()
    {
        await using var context = Context();
        await new WorkTaskRepository(context).GetProgressAsync(Guid.NewGuid(), CancellationToken.None);
    }

    [Fact]
    public async Task Control()
    {
        await using var context = Context();
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            context.Users.IgnoreQueryFilters().FirstOrDefaultAsync());
    }
}

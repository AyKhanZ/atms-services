using ATMS.Project.Data.Criteria.WorkTasks;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Project.Services.Tests.Criteria.WorkTasks;

public class WorkTasksOfLiveWorkCriteriaTest
{
    [Theory]
    [InlineData(false, false, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    public void Apply_KeepsOnlyTasksWhoseProjectAndTicketAreNotDeleted(
        bool projectDeleted, bool ticketDeleted, bool kept)
    {
        var task = new WorkTask
        {
            Id = Guid.NewGuid(),
            WorkProject = new WorkProject { IsDeleted = projectDeleted },
            WorkTicket = new WorkTicket { IsDeleted = ticketDeleted }
        };

        var result = new WorkTasksOfLiveWorkCriteria().Apply(new[] { task }.AsQueryable()).ToArray();

        Assert.Equal(kept, result.Length == 1);
    }

    [Fact]
    public void Apply_TranslatesToPostgreSql()
    {
        var options = new DbContextOptionsBuilder<ProjectDbContext>()
            .UseNpgsql("Host=localhost;Database=query-generation;Username=test")
            .Options;
        using var context = new ProjectDbContext(options);

        var sql = new WorkTasksOfLiveWorkCriteria().Apply(context.WorkTasks).ToQueryString();

        Assert.Contains("\"Projects\"", sql);
        Assert.Contains("\"Tickets\"", sql);
    }
}

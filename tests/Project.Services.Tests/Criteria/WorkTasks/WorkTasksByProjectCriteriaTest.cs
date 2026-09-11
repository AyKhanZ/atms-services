using ATMS.Project.Data.Criteria.WorkTasks;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Project.Services.Tests.Criteria.WorkTasks;

public class WorkTasksByProjectCriteriaTest
{
    [Theory]
    [InlineData("  Payment  ", "%Payment%")]
    [InlineData("STRIPE", "%STRIPE%")]
    [InlineData("оплата", "%оплата%")]
    [InlineData("51", "%51%")]
    [InlineData("50%_done", "%50\\%\\_done%")]
    public void Apply_WithSearch_TranslatesToScopedCaseInsensitiveSql(string search, string expectedPattern)
    {
        var options = new DbContextOptionsBuilder<ProjectDbContext>()
            .UseNpgsql("Host=localhost;Database=query-generation;Username=test")
            .Options;
        using var context = new ProjectDbContext(options);
        var projectId = Guid.NewGuid();
        var scopeId = Guid.NewGuid();
        var criteria = new WorkTasksByProjectCriteria(projectId, scopeId, null, true, search);

        var sql = criteria.Apply(context.WorkTasks).ToQueryString();

        Assert.Contains("ILIKE", sql);
        Assert.Contains("\"Code\"", sql);
        Assert.Contains("\"Title\"", sql);
        Assert.Contains(projectId.ToString(), sql);
        Assert.Contains(scopeId.ToString(), sql);
        Assert.Contains(expectedPattern, sql);
        Assert.Contains("\"ParentWorkTaskId\" IS NULL", sql);
    }

    [Theory]
    [InlineData(false, false, 3)]
    [InlineData(true, false, 2)]
    [InlineData(false, true, 1)]
    public void Apply_UsesRequestedHierarchyFilters(bool rootOnly, bool filterByParent, int expectedCount)
    {
        var projectId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var tasks = new[]
        {
            new WorkTask { WorkProjectId = projectId, WorkTicketId = ticketId },
            new WorkTask { WorkProjectId = projectId, WorkTicketId = ticketId },
            new WorkTask { WorkProjectId = projectId, WorkTicketId = ticketId, ParentWorkTaskId = parentId },
            new WorkTask { WorkProjectId = Guid.NewGuid(), WorkTicketId = ticketId }
        };

        var result = new WorkTasksByProjectCriteria(
                projectId,
                ticketId,
                filterByParent ? parentId : null,
                rootOnly)
            .Apply(tasks.AsQueryable())
            .ToArray();

        Assert.Equal(expectedCount, result.Length);
    }
}

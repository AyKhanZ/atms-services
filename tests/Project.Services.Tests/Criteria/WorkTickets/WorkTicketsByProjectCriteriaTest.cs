using ATMS.Project.Data.Criteria.WorkTickets;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Project.Services.Tests.Criteria.WorkTickets;

public class WorkTicketsByProjectCriteriaTest
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
        var criteria = new WorkTicketsByProjectCriteria(projectId, scopeId, search);

        var sql = criteria.Apply(context.WorkTickets).ToQueryString();

        Assert.Contains("ILIKE", sql);
        Assert.Contains("\"Code\"", sql);
        Assert.Contains("\"Title\"", sql);
        Assert.Contains(projectId.ToString(), sql);
        Assert.Contains(scopeId.ToString(), sql);
        Assert.Contains(expectedPattern, sql);
    }

    [Fact]
    public void Apply_WithoutMilestone_ReturnsOnlyProjectTickets()
    {
        var projectId = Guid.NewGuid();
        var tickets = CreateTickets(projectId);

        var result = new WorkTicketsByProjectCriteria(projectId, null)
            .Apply(tickets.AsQueryable())
            .ToArray();

        Assert.Equal(2, result.Length);
        Assert.All(result, ticket => Assert.Equal(projectId, ticket.WorkProjectId));
    }

    [Fact]
    public void Apply_WithMilestone_ReturnsOnlyProjectMilestoneTickets()
    {
        var projectId = Guid.NewGuid();
        var milestoneId = Guid.NewGuid();
        var tickets = CreateTickets(projectId, milestoneId);

        var result = new WorkTicketsByProjectCriteria(projectId, milestoneId)
            .Apply(tickets.AsQueryable())
            .ToArray();

        var ticket = Assert.Single(result);
        Assert.Equal(projectId, ticket.WorkProjectId);
        Assert.Equal(milestoneId, ticket.WorkGroupId);
    }

    private static WorkTicket[] CreateTickets(Guid projectId, Guid? milestoneId = null)
    {
        var firstMilestoneId = milestoneId ?? Guid.NewGuid();
        return
        [
            new WorkTicket { WorkProjectId = projectId, WorkGroupId = firstMilestoneId },
            new WorkTicket { WorkProjectId = projectId, WorkGroupId = Guid.NewGuid() },
            new WorkTicket { WorkProjectId = Guid.NewGuid(), WorkGroupId = firstMilestoneId }
        ];
    }
}

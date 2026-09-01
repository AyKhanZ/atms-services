using ATMS.Project.Data.Criteria.WorkTickets;
using ATMS.Project.Data.Entities;

namespace Project.Services.Tests.Criteria.WorkTickets;

public class WorkTicketsByProjectCriteriaTest
{
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

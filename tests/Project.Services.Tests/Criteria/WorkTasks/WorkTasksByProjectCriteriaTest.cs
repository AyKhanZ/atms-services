using ATMS.Project.Data.Criteria.WorkTasks;
using ATMS.Project.Data.Entities;

namespace Project.Services.Tests.Criteria.WorkTasks;

public class WorkTasksByProjectCriteriaTest
{
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

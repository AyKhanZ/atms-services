using ATMS.Project.Data.Criteria.WorkGroups;
using ATMS.Project.Data.Entities;

namespace Project.Services.Tests.Criteria.WorkGroups;

public class MilestonesByProjectCriteriaTest
{
    [Fact]
    public void Apply_WithoutSearch_ReturnsOnlyMilestonesFromProject()
    {
        var projectId = Guid.NewGuid();
        var groups = new[]
        {
            new WorkGroup { WorkProjectId = projectId, ParentWorkGroupId = Guid.NewGuid() },
            new WorkGroup { WorkProjectId = projectId, ParentWorkGroupId = null },
            new WorkGroup { WorkProjectId = Guid.NewGuid(), ParentWorkGroupId = Guid.NewGuid() }
        };

        var result = new MilestonesByProjectCriteria(projectId, null)
            .Apply(groups.AsQueryable())
            .ToArray();

        var milestone = Assert.Single(result);
        Assert.Equal(projectId, milestone.WorkProjectId);
        Assert.NotNull(milestone.ParentWorkGroupId);
    }
}

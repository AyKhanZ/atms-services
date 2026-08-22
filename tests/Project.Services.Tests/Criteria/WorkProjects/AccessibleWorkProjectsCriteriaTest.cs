using ATMS.Data.Constants;
using ATMS.Project.Data.Criteria.WorkProjects;
using ATMS.Project.Data.Entities;

namespace Project.Services.Tests.Criteria.WorkProjects;

public class AccessibleWorkProjectsCriteriaTest
{
    [Fact]
    public void Apply_WhenUserIsParticipant_ReturnsOnlyAccessibleProjects()
    {
        var userId = Guid.NewGuid();
        var accessibleProject = new WorkProject { Id = Guid.NewGuid() };
        accessibleProject.WorkProjectParticipants.Add(new WorkProjectParticipant { UserId = userId });
        var hiddenProject = new WorkProject { Id = Guid.NewGuid() };
        var criteria = new AccessibleWorkProjectsCriteria(userId, RoleIds.Employee);

        var result = criteria.Apply(new[] { accessibleProject, hiddenProject }.AsQueryable()).ToList();

        Assert.Single(result);
        Assert.Equal(accessibleProject.Id, result[0].Id);
    }

    [Fact]
    public void Apply_WhenUserIsSuperAdmin_ReturnsAllProjects()
    {
        var projects = new[]
        {
            new WorkProject { Id = Guid.NewGuid() },
            new WorkProject { Id = Guid.NewGuid() }
        };
        var criteria = new AccessibleWorkProjectsCriteria(Guid.NewGuid(), RoleIds.SuperAdmin);

        var result = criteria.Apply(projects.AsQueryable()).ToList();

        Assert.Equal(2, result.Count);
    }
}

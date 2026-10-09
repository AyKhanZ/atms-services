using ATMS.Project.Data.Criteria.Users;
using ATMS.Project.Data.Entities;

namespace Project.Services.Tests.Criteria.Users;

public class ActiveUsersCriteriaTest
{
    [Fact]
    public void Apply_LeavesOutInactiveUsers()
    {
        var active = new User { IsActive = true };
        var inactive = new User { IsActive = false };

        var result = new ActiveUsersCriteria().Apply(new[] { active, inactive }.AsQueryable()).ToArray();

        Assert.Equal(active.Id, Assert.Single(result).Id);
    }
}

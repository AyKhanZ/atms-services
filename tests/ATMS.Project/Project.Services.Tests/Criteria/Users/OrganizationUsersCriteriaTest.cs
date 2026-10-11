using ATMS.Project.Data.Criteria.Users;
using ATMS.Project.Data.Entities;

namespace Project.Services.Tests.Criteria.Users;

public class OrganizationUsersCriteriaTest
{
    [Fact]
    public void Apply_ReturnsUsersOfThatOrganization()
    {
        var organizationId = Guid.NewGuid();
        var member = new User { Id = Guid.NewGuid(), OrganizationId = organizationId };
        var outsider = new User { Id = Guid.NewGuid(), OrganizationId = Guid.NewGuid() };
        var withoutOrganization = new User { Id = Guid.NewGuid() };

        var result = new OrganizationUsersCriteria(organizationId)
            .Apply(new[] { member, outsider, withoutOrganization }.AsQueryable())
            .ToArray();

        Assert.Equal(member.Id, Assert.Single(result).Id);
    }
}

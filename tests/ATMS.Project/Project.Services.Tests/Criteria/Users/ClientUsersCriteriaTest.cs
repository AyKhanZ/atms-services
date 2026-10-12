using ATMS.Data.Enums;
using ATMS.Project.Data.Criteria.Users;
using ATMS.Project.Data.Entities;

namespace Project.Services.Tests.Criteria.Users;

public class ClientUsersCriteriaTest
{
    [Fact]
    public void Apply_ReturnsOnlyClients()
    {
        var client = new User { Id = Guid.NewGuid(), UserType = (int)UserTypeEnum.Client };
        var employee = new User { Id = Guid.NewGuid(), UserType = (int)UserTypeEnum.Employee };
        var superAdmin = new User { Id = Guid.NewGuid(), UserType = (int)UserTypeEnum.SuperAdmin };

        var result = new ClientUsersCriteria()
            .Apply(new[] { client, employee, superAdmin }.AsQueryable())
            .ToArray();

        Assert.Equal(client.Id, Assert.Single(result).Id);
    }
}

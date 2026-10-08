using ATMS.Data.Criteria.Users;
using ATMS.Data.Enums;
using ATMS.Project.Data.Criteria.Users;
using ATMS.Project.Data.Entities;

namespace Project.Services.Tests.Criteria.Users;

public class NotClientUsersCriteriaTest
{
    [Fact]
    public void Apply_WhenComposedWithNotAdmin_ReturnsOnlyEmployees()
    {
        var employee = new User { UserType = (int)UserTypeEnum.Employee };
        var client = new User { UserType = (int)UserTypeEnum.Client };
        var superAdmin = new User
        {
            UserType = (int)UserTypeEnum.SuperAdmin,
            IsAdmin = true
        };
        var users = new[] { employee, client, superAdmin }.AsQueryable();

        var result = new NotAdminCriteria<User>()
            .And(new NotClientUsersCriteria())
            .Apply(users)
            .ToArray();

        Assert.Equal(employee.Id, Assert.Single(result).Id);
    }
}

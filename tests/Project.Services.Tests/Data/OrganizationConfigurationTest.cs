using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Project.Services.Tests.Data;

public class OrganizationConfigurationTest
{
    [Theory]
    [InlineData(nameof(Organization.Title))]
    [InlineData(nameof(Organization.Voen))]
    public void UniqueIndex_OnlyIncludesActiveOrganizations(string propertyName)
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(Organization));
        var index = entityType!.GetIndexes().Single(candidate =>
            candidate.Properties.Count == 1 && candidate.Properties[0].Name == propertyName);

        Assert.True(index.IsUnique);
        Assert.Equal("\"IsDeleted\" = false", index.GetFilter());
    }

    private static ProjectDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ProjectDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ProjectDbContext(options);
    }
}

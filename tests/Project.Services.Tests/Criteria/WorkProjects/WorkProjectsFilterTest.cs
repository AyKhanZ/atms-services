using ATMS.Data.Enums;
using ATMS.Project.Data.Criteria.WorkProjects;
using ATMS.Project.Data.Entities;

namespace Project.Services.Tests.Criteria.WorkProjects;

public class WorkProjectsFilterTest
{
    [Fact]
    public void Apply_WhenDateAndDictionaryFiltersProvided_ReturnsMatchingProject()
    {
        var matchingProject = CreateProject("1", "Matching", new DateTime(2026, 2, 1));
        matchingProject.StartDate = new DateTime(2026, 2, 10, 0, 0, 0, DateTimeKind.Utc);
        matchingProject.EndDate = new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc);
        matchingProject.ProjectTypeId = 1;
        matchingProject.ProjectKindId = 2;
        matchingProject.ProjectStatusId = 3;
        var otherProject = CreateProject("2", "Other", new DateTime(2026, 2, 2));
        var filter = new WorkProjectsFilter
        {
            StartDate = new DateTime(2026, 2, 1),
            EndDate = new DateTime(2026, 3, 31),
            ProjectTypeId = 1,
            ProjectKindId = 2,
            ProjectStatusId = 3
        };

        var result = filter.Apply(new[] { matchingProject, otherProject }.AsQueryable()).ToList();

        Assert.Single(result);
        Assert.Equal(matchingProject.Id, result[0].Id);
    }

    [Fact]
    public void Apply_WhenSortingByCodeDescending_ReturnsExpectedOrder()
    {
        var projects = new[]
        {
            CreateProject("1", "First", new DateTime(2026, 1, 1)),
            CreateProject("2", "Second", new DateTime(2026, 2, 1))
        };
        var filter = new WorkProjectsFilter
        {
            SortBy = "code",
            SortDirection = SortDirectionEnum.Desc
        };

        var result = filter.Apply(projects.AsQueryable()).ToList();

        Assert.Equal(new[] { "2", "1" }, result.Select(x => x.Code));
    }

    private WorkProject CreateProject(string code, string title, DateTime createdAt)
    {
        return new WorkProject
        {
            Id = Guid.NewGuid(),
            Code = code,
            Title = title,
            CreatedAt = DateTime.SpecifyKind(createdAt, DateTimeKind.Utc)
        };
    }
}

using ATMS.Data.Enums;
using ATMS.Project.Data.Criteria.WorkProjects;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Project.Services.Tests.Criteria.WorkProjects;

public class WorkProjectsFilterTest
{
    [Theory]
    [InlineData(" Payment ", "%Payment%")]
    [InlineData("ОПЛАТА", "%ОПЛАТА%")]
    [InlineData("50%_done", "%50\\%\\_done%")]
    public void Apply_WithSearch_TranslatesToCaseInsensitiveLiteralSql(string search, string expectedPattern)
    {
        using var context = new ProjectDbContext(new DbContextOptionsBuilder<ProjectDbContext>()
            .UseNpgsql("Host=localhost;Database=query_translation_test;Username=test")
            .Options);

        var sql = new WorkProjectsFilter { Search = search }
            .Apply(context.WorkProjects.AsNoTracking()).ToQueryString();

        Assert.Contains(expectedPattern, sql);
        Assert.Contains("\"Title\" ILIKE", sql);
        Assert.Contains("\"Code\" ILIKE", sql);
        Assert.Contains("ESCAPE", sql);
    }

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

    // The code is a number kept as text, so plain text sorting would read #100 as smaller
    // than #99. Today every code has two digits and the fault is invisible.
    [Theory]
    [InlineData(SortDirectionEnum.Asc, new[] { "2", "33", "99", "100", "1000", "1023" })]
    [InlineData(SortDirectionEnum.Desc, new[] { "1023", "1000", "100", "99", "33", "2" })]
    public void Apply_WhenSortingByCode_OrdersCodesAsNumbers(
        SortDirectionEnum direction,
        string[] expected)
    {
        var projects = new[] { "100", "2", "1023", "99", "1000", "33" }
            .Select(code => CreateProject(code, code, new DateTime(2026, 1, 1)))
            .ToArray();
        var filter = new WorkProjectsFilter { SortBy = "code", SortDirection = direction };

        var result = filter.Apply(projects.AsQueryable()).ToList();

        Assert.Equal(expected, result.Select(x => x.Code));
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

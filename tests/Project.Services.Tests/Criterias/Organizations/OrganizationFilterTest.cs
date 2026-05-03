using ATMS.Data.Enums;
using ATMS.Project.Data.Criterias.Organizations;
using ATMS.Project.Data.Entities;

namespace Project.Services.Tests.Criterias.Organizations;

public class OrganizationFilterTest
{
    private static IQueryable<Organization> GetSampleData() =>
        new List<Organization>
        {
            new()
            {
                Id = Guid.NewGuid(), Title = "Alpha Corp", Voen = "1234567890",
                CreatedAt = new DateTime(2024, 1, 10, 0, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                Id = Guid.NewGuid(), Title = "Beta Ltd", Voen = "9876543210",
                CreatedAt = new DateTime(2024, 3, 15, 0, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                Id = Guid.NewGuid(), Title = "Alphabet Inc", Voen = "1111111111",
                CreatedAt = new DateTime(2024, 6, 20, 0, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                Id = Guid.NewGuid(), Title = "Gamma LLC", Voen = "2222222222",
                CreatedAt = new DateTime(2024, 9, 5, 0, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                Id = Guid.NewGuid(), Title = "Delta GmbH", Voen = "3333333333",
                CreatedAt = new DateTime(2024, 12, 1, 0, 0, 0, DateTimeKind.Utc)
            },
        }.AsQueryable();

    // Title filter

    [Fact]
    public void Apply_WhenTitleProvided_ShouldFilterByStartsWith()
    {
        var filter = new OrganizationFilter { Title = "al" };
        var result = filter.Apply(GetSampleData()).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, o => Assert.StartsWith("Al", o.Title, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Apply_WhenTitleHasWhitespace_ShouldTrimBeforeFiltering()
    {
        var filter = new OrganizationFilter { Title = "  Alphabet  " };
        var result = filter.Apply(GetSampleData()).ToList();

        Assert.Single(result);
        Assert.Equal("Alphabet Inc", result[0].Title);
    }

    [Fact]
    public void Apply_WhenTitleIsNull_ShouldNotFilterByTitle()
    {
        var filter = new OrganizationFilter { Title = null };
        var result = filter.Apply(GetSampleData()).ToList();

        Assert.Equal(5, result.Count);
    }

    [Fact]
    public void Apply_WhenTitleIsWhitespace_ShouldNotFilterByTitle()
    {
        var filter = new OrganizationFilter { Title = "   " };
        var result = filter.Apply(GetSampleData()).ToList();

        Assert.Equal(5, result.Count);
    }

    [Fact]
    public void Apply_WhenTitleDoesNotMatch_ShouldReturnEmpty()
    {
        var filter = new OrganizationFilter { Title = "ZZZ" };
        var result = filter.Apply(GetSampleData()).ToList();

        Assert.Empty(result);
    }

    // Voen filter

    [Fact]
    public void Apply_WhenVoenProvided_ShouldFilterByStartsWith()
    {
        var filter = new OrganizationFilter { Voen = "123" };
        var result = filter.Apply(GetSampleData()).ToList();

        Assert.Single(result);
        Assert.Equal("Alpha Corp", result[0].Title);
    }

    [Fact]
    public void Apply_WhenVoenIsNull_ShouldNotFilterByVoen()
    {
        var filter = new OrganizationFilter { Voen = null };
        var result = filter.Apply(GetSampleData()).ToList();

        Assert.Equal(5, result.Count);
    }

    [Fact]
    public void Apply_WhenVoenDoesNotMatch_ShouldReturnEmpty()
    {
        var filter = new OrganizationFilter { Voen = "0000" };
        var result = filter.Apply(GetSampleData()).ToList();

        Assert.Empty(result);
    }

    // Date range filter

    [Fact]
    public void Apply_WhenCreatedFromProvided_ShouldFilterOrganizationsOnOrAfter()
    {
        var filter = new OrganizationFilter { CreatedFrom = new DateTime(2024, 6, 1) };
        var result = filter.Apply(GetSampleData()).ToList();

        Assert.Equal(3, result.Count);
        Assert.All(result, o => Assert.True(o.CreatedAt >= new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc)));
    }

    [Fact]
    public void Apply_WhenCreatedToProvided_ShouldFilterOrganizationsOnOrBefore()
    {
        var filter = new OrganizationFilter { CreatedTo = new DateTime(2024, 3, 31) };
        var result = filter.Apply(GetSampleData()).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, o => Assert.True(o.CreatedAt <= new DateTime(2024, 3, 31, 0, 0, 0, DateTimeKind.Utc)));
    }

    [Fact]
    public void Apply_WhenBothDatesProvided_ShouldFilterByRange()
    {
        var filter = new OrganizationFilter
        {
            CreatedFrom = new DateTime(2024, 3, 1),
            CreatedTo = new DateTime(2024, 7, 1)
        };
        var result = filter.Apply(GetSampleData()).ToList();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Apply_WhenCreatedFromEqualsCreatedTo_ShouldReturnExactMatch()
    {
        var date = new DateTime(2024, 1, 10);
        var filter = new OrganizationFilter { CreatedFrom = date, CreatedTo = date };
        var result = filter.Apply(GetSampleData()).ToList();

        Assert.Single(result);
        Assert.Equal("Alpha Corp", result[0].Title);
    }

    // Sorting

    [Fact]
    public void Apply_WhenSortByTitleAsc_ShouldReturnAlphabeticalOrder()
    {
        var filter = new OrganizationFilter { SortBy = "title", SortDirection = SortDirectionEnum.Asc };
        var result = filter.Apply(GetSampleData()).ToList();

        var titles = result.Select(o => o.Title).ToList();
        Assert.Equal(titles.OrderBy(t => t).ToList(), titles);
    }

    [Fact]
    public void Apply_WhenSortByTitleDesc_ShouldReturnReverseAlphabeticalOrder()
    {
        var filter = new OrganizationFilter { SortBy = "title", SortDirection = SortDirectionEnum.Desc };
        var result = filter.Apply(GetSampleData()).ToList();

        var titles = result.Select(o => o.Title).ToList();
        Assert.Equal(titles.OrderByDescending(t => t).ToList(), titles);
    }

    [Fact]
    public void Apply_WhenSortByVoenAsc_ShouldReturnOrderedByVoen()
    {
        var filter = new OrganizationFilter { SortBy = "voen", SortDirection = SortDirectionEnum.Asc };
        var result = filter.Apply(GetSampleData()).ToList();

        var voens = result.Select(o => o.Voen).ToList();
        Assert.Equal(voens.OrderBy(v => v).ToList(), voens);
    }

    [Fact]
    public void Apply_WhenSortByVoenDesc_ShouldReturnOrderedByVoenDescending()
    {
        var filter = new OrganizationFilter { SortBy = "voen", SortDirection = SortDirectionEnum.Desc };
        var result = filter.Apply(GetSampleData()).ToList();

        var voens = result.Select(o => o.Voen).ToList();
        Assert.Equal(voens.OrderByDescending(v => v).ToList(), voens);
    }

    [Fact]
    public void Apply_WhenSortByIsNull_ShouldOrderByCreatedAtDescending()
    {
        var filter = new OrganizationFilter { SortBy = null };
        var result = filter.Apply(GetSampleData()).ToList();

        var dates = result.Select(o => o.CreatedAt).ToList();
        Assert.Equal(dates.OrderByDescending(d => d).ToList(), dates);
    }

    [Fact]
    public void Apply_WhenSortByIsUnknownField_ShouldFallbackToCreatedAtDescending()
    {
        var filter = new OrganizationFilter { SortBy = "unknownfield" };
        var result = filter.Apply(GetSampleData()).ToList();

        var dates = result.Select(o => o.CreatedAt).ToList();
        Assert.Equal(dates.OrderByDescending(d => d).ToList(), dates);
    }

    [Fact]
    public void Apply_WhenSortByTitleIsCaseInsensitive_ShouldStillSort()
    {
        var filterLower = new OrganizationFilter { SortBy = "title", SortDirection = SortDirectionEnum.Asc };
        var filterUpper = new OrganizationFilter { SortBy = "TITLE", SortDirection = SortDirectionEnum.Asc };
        var filterMixed = new OrganizationFilter { SortBy = "Title", SortDirection = SortDirectionEnum.Asc };

        var data = GetSampleData();
        var resultLower = filterLower.Apply(data).Select(o => o.Title).ToList();
        var resultUpper = filterUpper.Apply(data).Select(o => o.Title).ToList();
        var resultMixed = filterMixed.Apply(data).Select(o => o.Title).ToList();

        Assert.Equal(resultLower, resultUpper);
        Assert.Equal(resultLower, resultMixed);
    }

    // Combined filters

    [Fact]
    public void Apply_WhenTitleAndDateRangeCombined_ShouldApplyBothFilters()
    {
        var filter = new OrganizationFilter
        {
            Title = "al",
            CreatedFrom = new DateTime(2024, 5, 1),
            SortBy = "title",
            SortDirection = SortDirectionEnum.Asc
        };

        var result = filter.Apply(GetSampleData()).ToList();

        Assert.Single(result);
        Assert.Equal("Alphabet Inc", result[0].Title);
    }

    [Fact]
    public void Apply_WhenNoFiltersApplied_ShouldReturnAllOrderedByCreatedAtDesc()
    {
        var filter = new OrganizationFilter();
        var result = filter.Apply(GetSampleData()).ToList();

        Assert.Equal(5, result.Count);
        var dates = result.Select(o => o.CreatedAt).ToList();
        Assert.Equal(dates.OrderByDescending(d => d).ToList(), dates);
    }
}
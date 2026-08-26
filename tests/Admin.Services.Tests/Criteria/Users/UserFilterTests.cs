using ATMS.Admin.Data.Criteria.Users;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Entities.Dictionaries;
using ATMS.Data.Criteria;
using ATMS.Data.Enums;

namespace Admin.Services.Tests.Criteria.Users;

public class UserFilterTests
{
    #region Seed Data
 
    private static List<User> GetUsers() =>
    [
        new()
        {
            Id = Guid.NewGuid(), Name = "Ivan", Surname = "Petrov", Email = "ivan@mail.com",
            UserStatusId = 1, CreatedAt = new DateTime(2024, 1, 10),
            UserStatus = new UserStatus { Id = 1 }
        },
        new()
        {
            Id = Guid.NewGuid(), Name = "Anna", Surname = "Ivanova", Email = "anna@gmail.com",
            UserStatusId = 2, CreatedAt = new DateTime(2024, 3, 15),
            UserStatus = new UserStatus { Id = 2 }
        },
        new()
        {
            Id = Guid.NewGuid(), Name = "Igor", Surname = "Sidorov", Email = "igor@mail.com",
            UserStatusId = 1, CreatedAt = new DateTime(2024, 6, 20),
            UserStatus = new UserStatus { Id = 1 }
        },
        new()
        {
            Id = Guid.NewGuid(), Name = "Maria", Surname = "Petrova", Email = "maria@yahoo.com",
            UserStatusId = 3, CreatedAt = new DateTime(2024, 9, 5),
            UserStatus = new UserStatus { Id = 3 }
        },
        new()
        {
            Id = Guid.NewGuid(), Name = "Alexey", Surname = "Kozlov", Email = "alexey@mail.com",
            UserStatusId = 2, CreatedAt = new DateTime(2025, 1, 1),
            UserStatus = new UserStatus { Id = 2 }
        },
    ];
 
    private static IQueryable<User> GetQuery() => GetUsers().AsQueryable();
 
    #endregion
 
    #region Filter: Search
 
    [Fact]
    public void Apply_FilterBySearch_AddsSearchCriteria()
    {
        var filter = new UserFilter { Search = "Ivan" };
        var result = filter.Apply(GetQuery());
        Assert.Contains("ILike", result.Expression.ToString());
    }
 
    [Fact]
    public void Apply_FilterBySearch_IsCaseInsensitiveInProvider()
    {
        var filter = new UserFilter { Search = "ivan" };
        var result = filter.Apply(GetQuery());
        Assert.Contains("ILike", result.Expression.ToString());
    }
 
    [Fact]
    public void Apply_FilterBySearch_IgnoresLeadingSpaces()
    {
        var filter = new UserFilter { Search = "  Ivan" };
        var result = filter.Apply(GetQuery());
        Assert.Contains("ILike", result.Expression.ToString());
    }
 
    [Fact]
    public void Apply_FilterBySearch_EmptyString_ReturnsAll()
    {
        var filter = new UserFilter { Search = "" };
        var result = filter.Apply(GetQuery()).ToList();
        Assert.Equal(5, result.Count);
    }
 
    #endregion
 
    #region Filter: Surname
 
    [Fact]
    public void Apply_FilterBySearch_WhenMatchesSurname_AddsSearchCriteria()
    {
        var filter = new UserFilter { Search = "Petrov" };
        var result = filter.Apply(GetQuery());
        Assert.Contains("ILike", result.Expression.ToString());
    }
 
    [Fact]
    public void Apply_FilterBySearch_WhenMatchesSurname_IsCaseInsensitiveInProvider()
    {
        var filter = new UserFilter { Search = "petrov" };
        var result = filter.Apply(GetQuery());
        Assert.Contains("ILike", result.Expression.ToString());
    }
 
    #endregion
 
    #region Filter: Email
 
    [Fact]
    public void Apply_FilterBySearch_WhenMatchesEmail_AddsSearchCriteria()
    {
        var filter = new UserFilter { Search = "ivan@mail" };
        var result = filter.Apply(GetQuery());
        Assert.Contains("ILike", result.Expression.ToString());
    }
 
    [Fact]
    public void Apply_FilterBySearch_WhenMatchesEmail_IsCaseInsensitiveInProvider()
    {
        var filter = new UserFilter { Search = "IVAN@MAIL" };
        var result = filter.Apply(GetQuery());
        Assert.Contains("ILike", result.Expression.ToString());
    }
 
    #endregion
 
    #region Filter: UserStatusId
 
    [Fact]
    public void Apply_FilterByUserStatus_ReturnsMatchingUsers()
    {
        var filter = new UserFilter { UserStatusId = 1 };
        var result = filter.Apply(GetQuery()).ToList();
        Assert.Equal(2, result.Count);
    }
 
    [Fact]
    public void Apply_FilterByUserStatus_Zero_ReturnsAll()
    {
        var filter = new UserFilter { UserStatusId = 0 };
        var result = filter.Apply(GetQuery()).ToList();
        Assert.Equal(5, result.Count);
    }
 
    [Fact]
    public void Apply_FilterByUserStatus_Null_ReturnsAll()
    {
        var filter = new UserFilter { UserStatusId = null };
        var result = filter.Apply(GetQuery()).ToList();
        Assert.Equal(5, result.Count);
    }
 
    #endregion
 
    #region Filter: CreatedAt Range
 
    [Fact]
    public void Apply_FilterByCreatedFrom_ReturnsUsersAfterDate()
    {
        var filter = new UserFilter { CreatedFrom = new DateTime(2024, 6, 1) };
        var result = filter.Apply(GetQuery()).ToList();
        Assert.Equal(3, result.Count);
    }
 
    [Fact]
    public void Apply_FilterByCreatedTo_ReturnsUsersBeforeDate()
    {
        var filter = new UserFilter { CreatedTo = new DateTime(2024, 3, 31) };
        var result = filter.Apply(GetQuery()).ToList();
        Assert.Equal(2, result.Count);
    }
 
    [Fact]
    public void Apply_FilterByCreatedFromAndTo_ReturnsUsersInRange()
    {
        var filter = new UserFilter
        {
            CreatedFrom = new DateTime(2024, 3, 1),
            CreatedTo = new DateTime(2024, 9, 30)
        };
        var result = filter.Apply(GetQuery()).ToList();
        Assert.Equal(3, result.Count);
    }
 
    #endregion
 
    #region Sort
 
    [Fact]
    public void Apply_SortByName_Asc_ReturnsSortedAscending()
    {
        var filter = new UserFilter { SortBy = "name", SortDirection = SortDirectionEnum.Asc };
        var result = filter.Apply(GetQuery()).ToList();
        Assert.Equal("Alexey", result[0].Name);
    }
 
    [Fact]
    public void Apply_SortByName_Desc_ReturnsSortedDescending()
    {
        var filter = new UserFilter { SortBy = "name", SortDirection = SortDirectionEnum.Desc };
        var result = filter.Apply(GetQuery()).ToList();
        Assert.Equal("Maria", result[0].Name);
    }
 
    [Fact]
    public void Apply_SortBySurname_Asc_ReturnsSortedAscending()
    {
        var filter = new UserFilter { SortBy = "surname", SortDirection = SortDirectionEnum.Asc };
        var result = filter.Apply(GetQuery()).ToList();
        Assert.Equal("Ivanova", result[0].Surname);
    }
 
    [Fact]
    public void Apply_SortByEmail_Asc_ReturnsSortedAscending()
    {
        var filter = new UserFilter { SortBy = "email", SortDirection = SortDirectionEnum.Asc };
        var result = filter.Apply(GetQuery()).ToList();
        Assert.Equal("alexey@mail.com", result[0].Email);
    }
 
    [Fact]
    public void Apply_NoSortBy_DefaultSortByCreatedAtDesc()
    {
        var filter = new UserFilter();
        var result = filter.Apply(GetQuery()).ToList();
        Assert.Equal(new DateTime(2025, 1, 1), result[0].CreatedAt);
    }
 
    [Fact]
    public void Apply_UnknownSortBy_DefaultSortByCreatedAtDesc()
    {
        var filter = new UserFilter { SortBy = "unknown" };
        var result = filter.Apply(GetQuery()).ToList();
        Assert.Equal(new DateTime(2025, 1, 1), result[0].CreatedAt);
    }
 
    #endregion
 
    #region Combined Filters
 
    [Fact]
    public void Apply_SearchAndStatus_AddsBothCriteria()
    {
        var filter = new UserFilter { Search = "Iva", UserStatusId = 1 };
        var result = filter.Apply(GetQuery());
        var expression = result.Expression.ToString();
        Assert.Contains("ILike", expression);
        Assert.Contains("UserStatusId", expression);
    }
 
    [Fact]
    public void Apply_StatusAndDateRange_ReturnsCombinedResult()
    {
        var filter = new UserFilter
        {
            UserStatusId = 1,
            CreatedFrom = new DateTime(2024, 6, 1)
        };
        var result = filter.Apply(GetQuery()).ToList();
        Assert.Single(result);
        Assert.Equal("Igor", result[0].Name);
    }
 
    [Fact]
    public void Apply_AllFiltersAndSort_ReturnsCorrectResult()
    {
        var filter = new UserFilter
        {
            UserStatusId = 1,
            CreatedFrom = new DateTime(2024, 1, 1),
            CreatedTo = new DateTime(2024, 12, 31),
            SortBy = "name",
            SortDirection = SortDirectionEnum.Asc
        };
        var result = filter.Apply(GetQuery()).ToList();
        Assert.Equal(2, result.Count);
        Assert.Equal("Igor", result[0].Name);
        Assert.Equal("Ivan", result[1].Name);
    }
 
    #endregion
 
    #region Pagination
 
    [Fact]
    public void PaginationCriteria_Page1_ReturnsFirstItems()
    {
        var pagination = new PaginationCriteria<User>(1, 2);
        var result = pagination.Apply(GetQuery()).ToList();
        Assert.Equal(2, result.Count);
    }
 
    [Fact]
    public void PaginationCriteria_Page2_ReturnsNextItems()
    {
        var pagination = new PaginationCriteria<User>(2, 2);
        var result = pagination.Apply(GetQuery()).ToList();
        Assert.Equal(2, result.Count);
    }
 
    [Fact]
    public void PaginationCriteria_LastPage_ReturnsRemainingItems()
    {
        var pagination = new PaginationCriteria<User>(3, 2);
        var result = pagination.Apply(GetQuery()).ToList();
        Assert.Single(result);
    }
 
    [Fact]
    public void PaginationCriteria_PageBelowOne_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new PaginationCriteria<User>(0, 2));
    }
 
    [Fact]
    public void PaginationCriteria_PageSizeAboveMax_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new PaginationCriteria<User>(1, 999));
    }
 
    #endregion
}

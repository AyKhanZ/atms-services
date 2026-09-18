using ATMS.Data.Enums;
using ATMS.Project.Contracts.Commands.Search;
using ATMS.Project.Services.Validation.Search;

namespace Project.Services.Tests.Validators.Search;

public class RecordGlobalSearchRecentValidatorTest
{
    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(2, true)]
    [InlineData(3, true)]
    [InlineData(4, true)]
    [InlineData(5, false)]
    [InlineData(-1, false)]
    public void Validate_AcceptsOnlySupportedTypes(int type, bool valid)
    {
        var result = new RecordGlobalSearchRecentValidator().Validate(new RecordGlobalSearchRecentCommand
        {
            ItemType = type, ItemId = Guid.NewGuid()
        });

        Assert.Equal(valid, result.IsValid);
    }

    [Fact]
    public void Validate_EmptyIdReturnsFieldError()
    {
        var result = new RecordGlobalSearchRecentValidator().Validate(new RecordGlobalSearchRecentCommand
        {
            ItemType = (int)GlobalSearchItemType.Project, ItemId = Guid.Empty
        });

        Assert.Equal("ItemId", Assert.Single(result.Errors).PropertyName);
    }
}


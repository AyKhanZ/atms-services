using ATMS.Data.Enums;
using ATMS.Project.Contracts.Requests.Search;
using ATMS.Project.Services.Validation.Search;

namespace Project.Services.Tests.Validators.Search;

public class GetGlobalSearchPageValidatorTest
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
        var result = new GetGlobalSearchPageValidator().Validate(new GetGlobalSearchPageRequest
        {
            ItemType = type, Q = "payment"
        });

        Assert.Equal(valid, result.IsValid);
    }

    [Theory]
    [InlineData("41", true)]
    [InlineData("#41", true)]
    [InlineData("pay", true)]
    [InlineData("pa", false)]
    [InlineData("#pay", false)]
    [InlineData("   ", false)]
    [InlineData(null, false)]
    public void Validate_QueryIsCodeOrTitleOfThreeChars(string? query, bool valid)
    {
        var result = new GetGlobalSearchPageValidator().Validate(new GetGlobalSearchPageRequest
        {
            ItemType = (int)GlobalSearchItemTypeEnum.Task, Q = query
        });

        Assert.Equal(valid, result.IsValid);
    }
}

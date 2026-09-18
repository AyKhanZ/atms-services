using System.Globalization;
using ATMS.Project.Contracts.Requests.Search;
using ATMS.Project.Services.Validation.Search;

namespace Project.Services.Tests.Validators.Search;

public class GetGlobalSearchValidatorTest
{
    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("  ", true)]
    [InlineData("\t\r\n", true)]
    [InlineData(" x ", false)]
    [InlineData(" # ", false)]
    [InlineData("##", false)]
    [InlineData("###", false)]
    [InlineData("#1", true)]
    // A code is exact, so one digit is a usable query; a title is matched with "contains",
    // which only uses the trigram index from three characters up.
    [InlineData("1", true)]
    [InlineData("12", true)]
    [InlineData(" ab ", false)]
    [InlineData(" abc ", true)]
    public void Validate_QueryUsesTrimmedValue(string query, bool valid)
    {
        var result = new GetGlobalSearchValidator().Validate(new GetGlobalSearchRequest { Q = query });

        Assert.Equal(valid, result.IsValid);
        if (!valid)
        {
            Assert.Equal("Q", Assert.Single(result.Errors).PropertyName);
        }
    }

    [Theory]
    [InlineData(1, false)]
    [InlineData(2, false)]
    [InlineData(3, true)]
    [InlineData(100, true)]
    [InlineData(101, false)]
    public void Validate_QueryLengthBoundaries(int length, bool valid)
    {
        var result = new GetGlobalSearchValidator().Validate(new GetGlobalSearchRequest
        {
            Q = "  " + new string('a', length) + "  "
        });

        Assert.Equal(valid, result.IsValid);
    }

    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(50, true)]
    [InlineData(51, false)]
    public void Validate_TakeBoundaries(int take, bool valid)
    {
        var result = new GetGlobalSearchValidator().Validate(new GetGlobalSearchRequest { Take = take });

        Assert.Equal(valid, result.IsValid);
        if (!valid)
        {
            Assert.Equal("Take", Assert.Single(result.Errors).PropertyName);
        }
    }

    [Fact]
    public void Validate_InvalidQueryAndTakeReturnsBothFieldErrors()
    {
        var result = new GetGlobalSearchValidator().Validate(new GetGlobalSearchRequest { Q = "x", Take = 0 });

        Assert.Equal(2, result.Errors.Count);
        Assert.Contains(result.Errors, error => error.PropertyName == "Q");
        Assert.Contains(result.Errors, error => error.PropertyName == "Take");
    }

    [Theory]
    [InlineData("en", "Enter")]
    [InlineData("ru", "Введите")]
    [InlineData("az", "Ən azı")]
    public void Validate_TooLongQueryReturnsLocalizedValidation(string language, string expected)
    {
        var validator = new GetGlobalSearchValidator();
        var previous = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(language);
            var result = validator.Validate(new GetGlobalSearchRequest { Q = new string('a', 101) });

            Assert.StartsWith(expected, Assert.Single(result.Errors).ErrorMessage);
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }
}

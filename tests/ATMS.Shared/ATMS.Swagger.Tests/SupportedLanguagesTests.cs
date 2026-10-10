using ATMS.Application.Localization;

namespace ATMS.Swagger.Tests;

public class SupportedLanguagesTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("de")]
    [InlineData("fr,de;q=0.8")]
    public void FromAcceptLanguage_WithoutSupportedLanguage_ReturnsNull(string? header)
    {
        Assert.Null(SupportedLanguages.FromAcceptLanguage(header));
    }

    [Theory]
    [InlineData("ru", "ru")]
    [InlineData("ru-RU,ru;q=0.9,en;q=0.8", "ru")]
    [InlineData("fr,en;q=0.8", "en")]
    [InlineData("az-Latn-AZ", "az")]
    public void FromAcceptLanguage_ReturnsFirstSupportedLanguage(string header, string expected)
    {
        Assert.Equal(expected, SupportedLanguages.FromAcceptLanguage(header));
    }
}

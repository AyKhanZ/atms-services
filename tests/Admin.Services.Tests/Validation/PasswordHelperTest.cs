using ATMS.Application.Dispatcher.Validation;

namespace Admin.Services.Tests.Validation;

public sealed class PasswordHelperTest
{
    [Theory]
    [InlineData("Baim@1", 6, false)]
    [InlineData("Baim@2026!", 10, true)]
    public void IsValid_WhenPasswordMatchesRequirements_ReturnsTrue(
        string password,
        int minimumLength,
        bool requireLowercase)
    {
        var result = PasswordHelper.IsValid(password, minimumLength, requireLowercase);

        Assert.True(result);
    }

    [Theory]
    [InlineData("BAIM@2026!", 10, true)]
    [InlineData("Baim2026", 6, false)]
    [InlineData("Baim@2026 ", 6, false)]
    public void IsValid_WhenPasswordDoesNotMatchRequirements_ReturnsFalse(
        string password,
        int minimumLength,
        bool requireLowercase)
    {
        var result = PasswordHelper.IsValid(password, minimumLength, requireLowercase);

        Assert.False(result);
    }
}

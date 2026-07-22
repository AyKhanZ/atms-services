using ATMS.Admin.Contracts.Commands.Onboarding;
using ATMS.Admin.Service.Validation.Onboarding;

namespace Admin.Services.Tests.Validators.Onboarding;

public sealed class SaveSecurityValidatorTest
{
    private readonly SaveSecurityValidator _validator = new();

    [Fact]
    public async Task Validate_AcceptsStrongMatchingPasswords()
    {
        var result = await _validator.ValidateAsync(new SaveSecurityCommand
        {
            Password = "Baim@2026!",
            ConfirmPassword = "Baim@2026!",
            Version = 0
        });

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("weak", "weak")]
    [InlineData("Baim@2026!", "Other@2026!")]
    public async Task Validate_RejectsWeakOrMismatchedPasswords(string password, string confirmation)
    {
        var result = await _validator.ValidateAsync(new SaveSecurityCommand
        {
            Password = password,
            ConfirmPassword = confirmation,
            Version = 0
        });

        Assert.False(result.IsValid);
    }
}

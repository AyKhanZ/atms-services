using ATMS.Admin.Contracts.Commands.Profile;
using ATMS.Admin.Service.Validation.Profile;

namespace Admin.Services.Tests.Validators.Profile;

public class UpdateLanguageValidatorTest
{
    private readonly UpdateLanguageValidator _validator = new();

    [Fact]
    public async Task Validate_WhenValid_PassesValidation()
    {
        var command = new UpdateLanguageCommand { Language = "en" };

        var result = await _validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WhenLanguageEmpty_FailsValidation()
    {
        var command = new UpdateLanguageCommand { Language = "" };

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.Language));
    }

    [Fact]
    public async Task Validate_WhenLanguageTooShort_FailsValidation()
    {
        var command = new UpdateLanguageCommand { Language = "e" };

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.Language));
    }

    [Fact]
    public async Task Validate_WhenLanguageTooLong_FailsValidation()
    {
        var command = new UpdateLanguageCommand { Language = "eng" };

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.Language));
    }
}

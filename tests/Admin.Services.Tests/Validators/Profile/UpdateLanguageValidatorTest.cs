using ATMS.Admin.Contracts.Commands.Profile;
using ATMS.Admin.Service.Validation.Profile;

namespace Admin.Services.Tests.Validators.Profile;

public class UpdateLanguageValidatorTest
{
    private readonly UpdateLanguageValidator _validator = new();

    [Fact]
    public async Task Validate_WhenValid_PassesValidation()
    {
        var command = new UpdateLanguageCommand { Id = Guid.NewGuid(), Language = "en" };

        var result = await _validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WhenIdEmpty_FailsValidation()
    {
        var command = new UpdateLanguageCommand { Id = Guid.Empty, Language = "en" };

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.Id));
    }

    [Fact]
    public async Task Validate_WhenLanguageEmpty_FailsValidation()
    {
        var command = new UpdateLanguageCommand { Id = Guid.NewGuid(), Language = "" };

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.Language));
    }

    [Fact]
    public async Task Validate_WhenLanguageTooShort_FailsValidation()
    {
        var command = new UpdateLanguageCommand { Id = Guid.NewGuid(), Language = "e" };

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.Language));
    }

    [Fact]
    public async Task Validate_WhenLanguageTooLong_FailsValidation()
    {
        var command = new UpdateLanguageCommand { Id = Guid.NewGuid(), Language = "eng" };

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.Language));
    }
}
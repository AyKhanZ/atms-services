using System.ComponentModel.DataAnnotations;
using ATMS.Application.Localization;

namespace ATMS.Infrastructure.Options;

public sealed class LocalizationOptions
{
    [Required]
    [SupportedLanguage]
    public required string DefaultLanguage { get; init; }
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class SupportedLanguageAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string language &&
            SupportedLanguages.All.Contains(language, StringComparer.Ordinal))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(
            $"DefaultLanguage must be one of: {string.Join(", ", SupportedLanguages.All)}.");
    }
}

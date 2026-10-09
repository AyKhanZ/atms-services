using System.ComponentModel.DataAnnotations;
using ATMS.Application.Localization;

namespace ATMS.Infrastructure.Options;

public sealed class LocalizationOptions
{
    [Required]
    [AllowedValues(
        SupportedLanguages.English,
        SupportedLanguages.Russian,
        SupportedLanguages.Azerbaijani,
        ErrorMessage = "DefaultLanguage must be one of: en, ru, az.")]
    public required string DefaultLanguage { get; init; }
}

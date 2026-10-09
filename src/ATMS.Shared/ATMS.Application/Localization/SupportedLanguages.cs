namespace ATMS.Application.Localization;

public static class SupportedLanguages
{
    public const string English = "en";
    public const string Russian = "ru";
    public const string Azerbaijani = "az";

    public static readonly string[] All = [English, Russian, Azerbaijani];

    public static bool IsSupported(string? language) =>
        language is not null && All.Contains(language, StringComparer.OrdinalIgnoreCase);

    // english is the fallback translation, not the configured default language
    public static string Normalize(string? language) =>
        IsSupported(language) ? language!.ToLowerInvariant() : English;

    public static string ToCulture(string code) =>
        code switch
        {
            Russian => "ru-RU",
            Azerbaijani => "az-Latn-AZ",
            _ => "en-US"
        };
}

namespace ATMS.Application.Localization;

public static class SupportedLanguages
{
    public const string English = "en";
    public const string Russian = "ru";
    public const string Azerbaijani = "az";

    public static readonly string[] All = [English, Russian, Azerbaijani];

    public static bool IsSupported(string? language) =>
        language is not null && All.Contains(language, StringComparer.OrdinalIgnoreCase);

    // "ru-RU,ru;q=0.9,en;q=0.8" -> first supported language, or null
    public static string? FromAcceptLanguage(string? acceptLanguage)
    {
        if (string.IsNullOrWhiteSpace(acceptLanguage))
        {
            return null;
        }

        return acceptLanguage
            .Split(',')
            .Select(x => x.Split(';')[0].Trim())
            .Select(x => x.Length >= 2 ? x[..2].ToLowerInvariant() : x)
            .FirstOrDefault(IsSupported);
    }

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

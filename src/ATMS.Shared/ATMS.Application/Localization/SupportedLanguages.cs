namespace ATMS.Application.Localization;

public static class SupportedLanguages
{
    public const string English = "en";
    public const string Russian = "ru";
    public const string Azerbaijani = "az";

    public static readonly string[] All = [English, Russian, Azerbaijani];
    public static readonly string Default = English;
    
    public static bool IsSupported(string? language) =>
        language is not null && All.Contains(language, StringComparer.OrdinalIgnoreCase);

    public static string Normalize(string? language) =>
        IsSupported(language) ? language!.ToLower() : Default;
}

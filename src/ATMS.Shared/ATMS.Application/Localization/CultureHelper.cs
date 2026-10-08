namespace ATMS.Application.Localization;

public static class CultureHelper
{
    // use this in services instead of IHttpContextAccessor
    public static string CurrentLanguage =>
        SupportedLanguages.Normalize(Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName);
}

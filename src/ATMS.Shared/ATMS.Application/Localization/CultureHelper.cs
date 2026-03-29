namespace ATMS.Application.Localization;

public static class CultureHelper
{
    /// <summary>
    /// Returns the current language from the thread's CultureInfo.
    /// Always use this in services instead of accessing IHttpContextAccessor directly.
    /// </summary>
    public static string CurrentLanguage =>
        SupportedLanguages.Normalize(Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName);
}

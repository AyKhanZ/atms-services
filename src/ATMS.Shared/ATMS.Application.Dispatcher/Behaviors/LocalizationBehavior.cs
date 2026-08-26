using System.Globalization;
using ATMS.Application.Localization;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ATMS.Application.Dispatcher.Behaviors;

/// <summary>
/// Sets the thread culture before executing the request.
/// Must be placed before access and validation behaviors so localized resources use the request language.
///
/// Language priority:
/// 1. Accept-Language header
/// 2. "en" — Default
/// </summary>
public sealed class LocalizationBehavior<TRequest, TResponse>(
    IHttpContextAccessor httpContextAccessor,
    ILogger<LocalizationBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var language = ResolveLanguage();
        SetCulture(language);

        logger.LogDebug("Localization set to '{Language}' for {Request}",
            language, typeof(TRequest).Name);

        return await next(cancellationToken);
    }

    private string ResolveLanguage()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            return SupportedLanguages.Default;
        }

        // 1. Accept-Language header (standard HTTP)
        var acceptLanguage = httpContext.Request.Headers["Accept-Language"].ToString();
        if (string.IsNullOrWhiteSpace(acceptLanguage))
        {
            return SupportedLanguages.Default;
        }
        // Parse "ru-RU,ru;q=0.9,en;q=0.8" → take first supported
        var parsed = acceptLanguage
            .Split(',')
            .Select(x => x.Split(';')[0].Trim()) // remove q-factor
            .Select(x => x.Length >= 2 ? x[..2].ToLower() : x) // "ru-RU" → "ru"
            .FirstOrDefault(SupportedLanguages.IsSupported);

        return parsed ??
               // 2. Default
               SupportedLanguages.Default;
    }

    private void SetCulture(string language)
    {
        var culture = new CultureInfo(language switch
        {
            SupportedLanguages.Russian => "ru-RU",
            SupportedLanguages.Azerbaijani => "az-Latn-AZ",
            _ => "en-US"
        });

        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture; // ← affects .resx lookup
    }
}

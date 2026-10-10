using System.Globalization;
using ATMS.Application.Localization;
using ATMS.Infrastructure.Options;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ATMS.Application.Dispatcher.Behaviors;

/// <summary>
/// Language priority:
/// 1. Accept-Language header
/// 2. LocalizationOptions.DefaultLanguage
/// </summary>
// must be registered before access and validation, so their messages use the request language
public sealed class LocalizationBehavior<TRequest, TResponse>(
    IHttpContextAccessor httpContextAccessor,
    IOptions<LocalizationOptions> localizationOptions,
    ILogger<LocalizationBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly string defaultLanguage = localizationOptions.Value.DefaultLanguage;

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
            return defaultLanguage;
        }

        // 1. Accept-Language header, 2. configured default
        var acceptLanguage = httpContext.Request.Headers.AcceptLanguage.ToString();
        return SupportedLanguages.FromAcceptLanguage(acceptLanguage) ?? defaultLanguage;
    }

    private static void SetCulture(string language)
    {
        var culture = new CultureInfo(SupportedLanguages.ToCulture(language));

        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture; // used by the .resx lookup
    }
}

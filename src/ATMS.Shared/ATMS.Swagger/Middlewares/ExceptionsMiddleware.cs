using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Exceptions.Configuration;
using Newtonsoft.Json;
using System.Net;
using ATMS.Application.Exceptions.Auth;
using ATMS.Application.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ATMS.Swagger.Middlewares;

public class ExceptionsMiddleware(ILogger<ExceptionsMiddleware> logger) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (AuthException ex)
        {
            await HandleExceptionAsync(context, ex);
        }
        catch (EntityException ex)
        {
            await HandleExceptionAsync(context, ex);
        }
        catch (ConfigurationException ex)
        {
            await HandleExceptionAsync(context, ex);
        }
        catch (ValidationException ex)
        {
            await HandleExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, AuthException exception)
    {
        logger.LogWarning(exception, "Authentication error: {Message}", exception.Message);

        var code = HttpStatusCode.InternalServerError;

        switch (exception.AuthErrorType)
        {
            case AuthErrorType.InvalidToken:
            case AuthErrorType.InvalidCredentials:
            case AuthErrorType.EmailNotConfirmed:
                code = HttpStatusCode.Unauthorized;
                break;
            case AuthErrorType.AccountLocked:
                code = HttpStatusCode.Locked;
                break;
            case AuthErrorType.AccountInactive:
                code = HttpStatusCode.Forbidden;
                break;
            case AuthErrorType.EmailAlreadyConfirmed:
                code = HttpStatusCode.Conflict;
                break;
            case AuthErrorType.TokenGenerationFailed:
                logger.LogError(exception, "Authentication error: {Message}", exception.Message);
                break;
        }

        var result = JsonConvert.SerializeObject(new { error = exception.Message });
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        return context.Response.WriteAsync(result);
    }

    private Task HandleExceptionAsync(HttpContext context, EntityException exception)
    {
        logger.LogWarning(exception, "Entity error: {Message}", exception.Message);

        var code = HttpStatusCode.InternalServerError;

        switch (exception.ErrorType)
        {
            case EntityErrorType.NotFound:
                code = HttpStatusCode.NotFound;
                break;
        }

        var result = JsonConvert.SerializeObject(new { error = exception.Message });
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        return context.Response.WriteAsync(result);
    }

    private Task HandleExceptionAsync(HttpContext context, ConfigurationException exception)
    {
        logger.LogCritical(exception, "Configuration error on {Path} {Method}: {Message}",
            context.Request.Path, context.Request.Method, exception.Message);

        var result = JsonConvert.SerializeObject(new { error = "System is not properly initialized. Please contact support." });
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        return context.Response.WriteAsync(result);
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        logger.LogCritical(exception, "Unexpected exception: {Message}", exception.Message);

        var result = JsonConvert.SerializeObject(new { error = "Internal server error" });
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        return context.Response.WriteAsync(result);
    }

    private Task HandleExceptionAsync(HttpContext context, ValidationException exception)
    {
        logger.LogError(exception,
            "Validation error. Count: {Count}. Errors: {@Errors}",
            exception.Errors.Count(),
            exception.Errors.Select(f => new
            {
                f.PropertyName,
                f.ErrorMessage
            }));

        var response = new ValidationErrorModel
        {
            Errors = exception.Errors.Select(f => new FieldError
            {
                Field = f.PropertyName,
                Error = f.ErrorMessage
            }).ToList()
        };
        var result = JsonConvert.SerializeObject(response);
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        return context.Response.WriteAsync(result);
    }
}
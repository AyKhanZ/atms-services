using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Exceptions.Configuration;
using Newtonsoft.Json;
using System.Net;
using ATMS.Application.Exceptions.Auth;
using ATMS.Application.Exceptions.Conflict;
using ATMS.Application.Exceptions.Image;
using ATMS.Application.Exceptions.Resources;
using ATMS.Application.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

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
        catch (ImageException ex)
        {
            await HandleExceptionAsync(context, ex);
        }
        catch (ValidationException ex)
        {
            await HandleExceptionAsync(context, ex);
        }
        catch (ConflictException ex)
        {
            await HandleExceptionAsync(context, ex);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            await HandleUniqueConstraintViolationAsync(context, ex);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            return;
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, AuthException exception)
    {
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
            case AuthErrorType.Forbidden:
            case AuthErrorType.AccountInactive:
                code = HttpStatusCode.Forbidden;
                break;
            case AuthErrorType.EmailAlreadyConfirmed:
                code = HttpStatusCode.Conflict;
                break;
            case AuthErrorType.TokenGenerationFailed:
                logger.LogError(exception, "Authentication error: {Message}", exception.Message);
                break;
            default:
                logger.LogError(exception, "Unhandled auth error type: {Type}", exception.AuthErrorType);
                break;
        }

        var errorMessage = code == HttpStatusCode.InternalServerError
            ? ExceptionMessages.InternalServerError
            : exception.Message;

        var result = JsonConvert.SerializeObject(new { error = errorMessage });
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        return context.Response.WriteAsync(result);
    }

    private Task HandleExceptionAsync(HttpContext context, EntityException exception)
    {
        var code = HttpStatusCode.InternalServerError;

        switch (exception.ErrorType)
        {
            case EntityErrorType.NotFound:
                code = HttpStatusCode.NotFound;
                break;
            default:
                logger.LogError(exception, "Unhandled entity error type: {Type}", exception.ErrorType);
                break;
        }

        var errorMessage = code == HttpStatusCode.InternalServerError
            ? ExceptionMessages.InternalServerError
            : exception.Message;

        var result = JsonConvert.SerializeObject(new { error = errorMessage });
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        return context.Response.WriteAsync(result);
    }

    private Task HandleExceptionAsync(HttpContext context, ConfigurationException exception)
    {
        logger.LogCritical(exception, "Configuration error on {Path} {Method}: {Message}",
            context.Request.Path, context.Request.Method, exception.Message);

        var result = JsonConvert.SerializeObject(new { error = ExceptionMessages.InternalServerError });
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        return context.Response.WriteAsync(result);
    }

    private Task HandleExceptionAsync(HttpContext context, ImageException exception)
    {
        var response = new ValidationErrorModel
        {
            Errors =
            [
                new FieldError
                {
                    Field = exception.PropertyName,
                    Error = exception.UserMessage
                }
            ]
        };

        var result = JsonConvert.SerializeObject(response);
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        return context.Response.WriteAsync(result);
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Разделяем — ошибка БД это не то же самое что NullReferenceException
        if (exception is NpgsqlException or TimeoutException)
        {
            logger.LogError(exception,
                "Infrastructure error. RequestId: {RequestId}, Path: {Path}, Message: {Message}",
                context.TraceIdentifier,
                context.Request.Path,
                exception.Message);

            var result = JsonConvert.SerializeObject(new
            {
                error = ExceptionMessages.ServiceTemporarilyUnavailable,
                requestId = context.TraceIdentifier
            });
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable; // 503
            return context.Response.WriteAsync(result);
        }

        // Unexpected exception — unhandled case, needs immediate investigation 500
        logger.LogError(exception,
            "Unexpected exception. RequestId: {RequestId}, Path: {Path}, Method: {Method}, Message: {Message}",
            context.TraceIdentifier,
            context.Request.Path,
            context.Request.Method,
            exception.Message);

        var errorResult = JsonConvert.SerializeObject(new
        {
            error = ExceptionMessages.InternalServerError,
            requestId = context.TraceIdentifier
        });
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        return context.Response.WriteAsync(errorResult);
    }

    private Task HandleExceptionAsync(HttpContext context, ValidationException exception)
    {
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

    private Task HandleExceptionAsync(HttpContext context, ConflictException exception)
    {
        logger.LogInformation(
            "Request conflict. RequestId: {RequestId}, Path: {Path}, Message: {Message}",
            context.TraceIdentifier,
            context.Request.Path,
            exception.Message);

        var result = JsonConvert.SerializeObject(new { error = exception.Message });
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status409Conflict;
        return context.Response.WriteAsync(result);
    }

    private Task HandleUniqueConstraintViolationAsync(
        HttpContext context,
        DbUpdateException exception)
    {
        logger.LogInformation(
            exception,
            "Unique constraint conflict. RequestId: {RequestId}, Path: {Path}",
            context.TraceIdentifier,
            context.Request.Path);

        var result = JsonConvert.SerializeObject(new
        {
            error = ExceptionMessages.NameAlreadyInUse
        });
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status409Conflict;

        return context.Response.WriteAsync(result);
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException exception)
    {
        return exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation
        };
    }
}

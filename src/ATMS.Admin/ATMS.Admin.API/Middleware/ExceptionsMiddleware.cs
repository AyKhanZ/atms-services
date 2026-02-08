using ATMS.Admin.API.Middleware.Exceptions.Auth;
using ATMS.Admin.Service.Behaviors.Models;
using ATMS.Shared.Exceptions.Entity;
using Newtonsoft.Json;
using System.Net;

namespace ATMS.Admin.API.Middleware;

public class ExceptionsMiddleware : IMiddleware
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
        catch (FluentValidation.ValidationException ex)
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
        var code = HttpStatusCode.InternalServerError;

        switch (exception.AuthErrorType)
        {
            case AuthErrorType.InvalidToken:
            case AuthErrorType.InvalidRefreshToken:
            case AuthErrorType.InvalidCredentials:
            case AuthErrorType.EmailNotConfirmed:
            case AuthErrorType.PasswordMismatch:
                code = HttpStatusCode.Unauthorized;
                break;
            case AuthErrorType.EmailAlreadyConfirmed:
                code = HttpStatusCode.NoContent;
                break;
        }

        var result = JsonConvert.SerializeObject(new { error = exception.Message });
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        return context.Response.WriteAsync(result);
    }

    private Task HandleExceptionAsync(HttpContext context, EntityException exception)
    {
        var code = HttpStatusCode.InternalServerError;

        switch (exception.EntityErrorType)
        {
            case EntityErrorType.EntityNotFound:
                code = HttpStatusCode.NotFound;
                break;
        }

        var result = JsonConvert.SerializeObject(new { error = exception.Message });
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        return context.Response.WriteAsync(result);
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var code = HttpStatusCode.InternalServerError;

        var result = JsonConvert.SerializeObject(new { error = exception.Message });
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        return context.Response.WriteAsync(result);
    }

    private Task HandleExceptionAsync(HttpContext context, FluentValidation.ValidationException exception)
    {
        var code = HttpStatusCode.BadRequest;

        var response = new ValidationErrorResponse
        {
            Errors = exception.Errors.Select(f => new FieldError
            {
                Field = f.PropertyName,
                Error = f.ErrorMessage
            }).ToList()
        };
        var result = JsonConvert.SerializeObject(response);
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        return context.Response.WriteAsync(result);
    }
}

using ATMS.Application.Exceptions.Configuration;
using ATMS.Application.Exceptions.Resources;
using ATMS.Swagger.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace Admin.API.Tests;

public class ExceptionsMiddlewareTest
{
    [Theory]
    [MemberData(nameof(UnexpectedExceptions))]
    public async Task InvokeAsync_WhenUnexpectedExceptionOccurs_ReturnsSafeInternalServerError(
        Exception exception)
    {
        var context = CreateContext();
        var middleware = new ExceptionsMiddleware(NullLogger<ExceptionsMiddleware>.Instance);

        await middleware.InvokeAsync(context, _ => throw exception);

        var response = await ReadResponseAsync(context);
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Contains(ExceptionMessages.InternalServerError, response);
        Assert.DoesNotContain(exception.Message, response);
        Assert.DoesNotContain(exception.GetType().Name, response);
    }

    [Fact]
    public async Task InvokeAsync_WhenConfigurationExceptionOccurs_ReturnsSafeInternalServerError()
    {
        var context = CreateContext();
        var middleware = new ExceptionsMiddleware(NullLogger<ExceptionsMiddleware>.Instance);
        var exception = new ConfigurationException(
            ConfigurationErrorType.MissingSeedData,
            "Sensitive configuration details");

        await middleware.InvokeAsync(context, _ => throw exception);

        var response = await ReadResponseAsync(context);
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Contains(ExceptionMessages.InternalServerError, response);
        Assert.DoesNotContain(exception.Message, response);
    }

    public static TheoryData<Exception> UnexpectedExceptions()
    {
        return new TheoryData<Exception>
        {
            new NullReferenceException("Internal null reference details"),
            new ArgumentException("Internal argument details")
        };
    }

    private DefaultHttpContext CreateContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private async Task<string> ReadResponseAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        return await reader.ReadToEndAsync();
    }
}

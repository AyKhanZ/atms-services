using System.Net;
using System.Net.Http.Json;
using System.Text;
using ATMS.Admin.Contracts.Models;
using ATMS.Admin.Service.Modules;
using ATMS.Admin.Service.Providers;
using ATMS.Application.Infrastructure;
using ATMS.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Admin.Services.Tests.Providers;

public class MigrationsProviderTest
{
    [Fact]
    public async Task ApplyAsync_ReturnsProjectModelWhenCallSucceeds()
    {
        var expected = new MigrationModel
        {
            AppliedMigrations = ["ProjectMigration"]
        };
        var provider = CreateProvider(request =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("/api/v1/migrations/up", request.RequestUri?.PathAndQuery);

            return JsonResponse(HttpStatusCode.OK, expected);
        });

        var result = await provider.ApplyAsync(CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(expected.AppliedMigrations, result.AppliedMigrations);
    }

    [Fact]
    public async Task ApplyAsync_ReturnsErrorMessageWhenProjectReturnsFailureStatus()
    {
        var provider = CreateProvider(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent(
                """{"code":"error","message":"Project database is unavailable."}""",
                Encoding.UTF8,
                "application/json")
        });

        var result = await provider.ApplyAsync(CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("Project database is unavailable.", result.ErrorMessage);
    }

    [Fact]
    public async Task ApplyAsync_ReturnsExceptionMessageWhenProjectIsDown()
    {
        var provider = CreateProvider(_ => throw new HttpRequestException("Connection refused."));

        var result = await provider.ApplyAsync(CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("Connection refused.", result.ErrorMessage);
    }

    [Fact]
    public void MigrationsClient_WaitsFiveMinutes()
    {
        var services = new ServiceCollection();
        services.AddHttpContextAccessor();
        services.AddTransient<AuthorizationDelegatingHandler>();
        services.AddProviderServices(BuildConfiguration());

        using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient(MigrationsProvider.HttpClientName);

        Assert.Equal(TimeSpan.FromMinutes(5), client.Timeout);
        Assert.Equal(new Uri("http://project-api:8080"), client.BaseAddress);
    }

    private static MigrationsProvider CreateProvider(
        Func<HttpRequestMessage, HttpResponseMessage> respond)
    {
        var handler = new StubHandler(respond);
        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://project-api:8080")
        };
        var factory = new SingleClientFactory(client);

        return new MigrationsProvider(factory);
    }

    private static HttpResponseMessage JsonResponse(HttpStatusCode statusCode, MigrationModel model)
    {
        var response = new HttpResponseMessage(statusCode)
        {
            Content = JsonContent.Create(model)
        };

        return response;
    }

    private static IConfiguration BuildConfiguration() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{nameof(ProviderOptions)}:{nameof(ProviderOptions.AdminServiceUrl)}"] = "http://admin-api:8080",
                [$"{nameof(ProviderOptions)}:{nameof(ProviderOptions.ProjectServiceUrl)}"] = "http://project-api:8080",
                [$"{nameof(ProviderOptions)}:{nameof(ProviderOptions.TimeoutSeconds)}"] = "5"
            })
            .Build();

    private sealed class SingleClientFactory(HttpClient client) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            Assert.Equal(MigrationsProvider.HttpClientName, name);
            return client;
        }
    }

    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(respond(request));
        }
    }
}

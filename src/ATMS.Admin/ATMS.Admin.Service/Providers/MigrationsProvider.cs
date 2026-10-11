using System.Net.Http.Json;
using System.Text.Json;
using ATMS.Admin.Contracts.Models;
using ATMS.Admin.Service.Providers.Interfaces;
using ATMS.Application.Models;

namespace ATMS.Admin.Service.Providers;

public sealed class MigrationsProvider(IHttpClientFactory factory) : IMigrationsProvider
{
    public const string HttpClientName = "ProjectMigrationsClient";

    public async Task<MigrationModel> ApplyAsync(CancellationToken cancellationToken)
    {
        try
        {
            var client = factory.CreateClient(HttpClientName);
            var response = await client.PostAsync("api/v1/migrations/up", content: null, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new MigrationModel
                {
                    ErrorMessage = await ReadErrorMessageAsync(response, cancellationToken)
                };
            }

            var model = await response.Content.ReadFromJsonAsync<MigrationModel>(cancellationToken);

            return model ?? new MigrationModel
            {
                ErrorMessage = "Project migrations returned an empty response."
            };
        }
        catch (Exception exception) when (
            !cancellationToken.IsCancellationRequested
            && exception is HttpRequestException or TimeoutException or TaskCanceledException or JsonException)
        {
            return new MigrationModel
            {
                ErrorMessage = exception.Message
            };
        }
    }

    private static async Task<string> ReadErrorMessageAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(body))
        {
            return $"Project migrations failed with status {(int)response.StatusCode}.";
        }

        try
        {
            var error = JsonSerializer.Deserialize<ErrorModel>(body, JsonSerializerOptions.Web);

            if (!string.IsNullOrWhiteSpace(error?.Message))
            {
                return error.Message;
            }
        }
        catch (JsonException)
        {
            // The body is the error text when it is not the API error shape.
        }

        return body;
    }
}

using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddHealthChecks();
builder.Services.AddHttpClient();

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.MapHealthChecks("/health/live");

app.MapGet("/", () => Results.Redirect("/swagger"));

app.MapGet("/swagger/admin/swagger.json", (
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        CancellationToken cancellationToken) =>
    GetSwaggerDocumentAsync(
        httpClientFactory,
        configuration,
        clusterId: "admin-api",
        destinationId: "admin-api-http",
        gatewayServerUrl: "/admin",
        cancellationToken));

app.MapGet("/swagger/project/swagger.json", (
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        CancellationToken cancellationToken) =>
    GetSwaggerDocumentAsync(
        httpClientFactory,
        configuration,
        clusterId: "project-api",
        destinationId: "project-api-http",
        gatewayServerUrl: "/project",
        cancellationToken));

app.UseSwaggerUI(options =>
{
    options.RoutePrefix = "swagger";
    options.DocumentTitle = "ATMS Gateway Swagger";
    options.SwaggerEndpoint("/swagger/admin/swagger.json", "Admin API");
    options.SwaggerEndpoint("/swagger/project/swagger.json", "Project API");
});

app.MapReverseProxy();

app.Run();

static async Task<IResult> GetSwaggerDocumentAsync(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    string clusterId,
    string destinationId,
    string gatewayServerUrl,
    CancellationToken cancellationToken)
{
    var downstreamAddress = configuration[
        $"ReverseProxy:Clusters:{clusterId}:Destinations:{destinationId}:Address"];

    if (string.IsNullOrWhiteSpace(downstreamAddress))
    {
        return Results.Problem($"Gateway destination '{clusterId}/{destinationId}' is not configured.");
    }

    var swaggerUri = new Uri(new Uri(downstreamAddress), "swagger/v1/swagger.json");
    var json = await httpClientFactory
        .CreateClient()
        .GetStringAsync(swaggerUri, cancellationToken);

    var document = JsonNode.Parse(json)?.AsObject();
    if (document is null)
    {
        return Results.Problem($"Invalid swagger document returned from '{swaggerUri}'.");
    }

    document["servers"] = new JsonArray(
        new JsonObject
        {
            ["url"] = gatewayServerUrl
        });

    return Results.Text(
        document.ToJsonString(new JsonSerializerOptions(JsonSerializerDefaults.Web)),
        "application/json");
}

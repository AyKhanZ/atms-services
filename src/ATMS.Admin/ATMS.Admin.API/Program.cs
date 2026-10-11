using ATMS.Admin.API.Extensions;
using ATMS.Admin.Service.Modules;
using ATMS.Swagger.Constants;
using ATMS.Swagger.Extensions;
using ATMS.Swagger.Middlewares;
using ATMS.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services
    .AddApiServices()
    .AddCustomMiddlewares()
    .AddAdminServices(builder.Configuration)
    .AddJwtSecurityServices(builder.Configuration)
    .AddAuthorizationPolicies()
    .AddRateLimitingPolicies(builder.Configuration)
    .AddSwaggerDocumentation(SwaggerConstants.ApiAdminTitle);

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseProxyForwardedHeaders(builder.Configuration);

// off on a client server; the demo turns it on with SwaggerOptions:Enabled to try the API by hand
if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("SwaggerOptions:Enabled"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.UseLocalImageFiles(builder.Configuration);

app.UseMiddleware<ExceptionsMiddleware>();

app.UseAuthentication();

app.UseAuthorization();

app.UseRateLimiter();

app.MapControllers();

await app.InitializeDataAsync();

app.Run();

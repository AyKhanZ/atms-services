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
    .AddSwaggerDocumentation(SwaggerConstants.ApiAdminTitle);

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.UseLocalImageFiles(builder.Configuration);

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseMiddleware<ExceptionsMiddleware>();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

await app.InitializeDataAsync();
await app.InitializeEventBusAsync();

app.Run();

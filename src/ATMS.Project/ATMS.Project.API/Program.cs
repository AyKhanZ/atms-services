using ATMS.Infrastructure.Extensions;
using ATMS.Swagger.Extensions;
using ATMS.Swagger.Middlewares;
using ATMS.Project.Services.Modules;
using ATMS.Swagger.Constants;
using ATMS.Application.Realtime;
using ATMS.Project.API.Hubs;
using ATMS.Project.API.Realtime;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services
    .AddApiServices()
    .AddCustomMiddlewares()
    .AddProjectServices(builder.Configuration)
    .AddJwtSecurityServices(builder.Configuration)
    .AddAuthorizationPolicies()
    .AddRealtime()
    .AddSwaggerDocumentation(SwaggerConstants.ApiProjectTitle);

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
app.MapHub<RealtimeHub>(RealtimeConstants.HubPath);

app.Run();

using ATMS.Admin.API.Extensions;
using ATMS.Admin.API.Middleware;
using ATMS.Admin.Service.Modules;
using ATMS.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services
    .AddConfigurations(builder.Configuration)
    .AddApiServices()
    .AddCustomMiddlewares()
    .AddAdminServices()
    .AddJwtSecurityServices(builder.Configuration)
    .AddAuthorizationPolicies()
    .AddSwaggerDocumentation();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionsMiddleware>();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

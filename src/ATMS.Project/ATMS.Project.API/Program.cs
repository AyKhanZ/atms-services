using ATMS.Project.API.Extensions;
using ATMS.Infrastructure.Extensions;
using ATMS.Swagger.Extensions;
using ATMS.Swagger.Middlewares;
using ATMS.Project.Services.Modules;
using ATMS.Swagger.Constants;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services
    .AddApiServices()
    .AddCustomMiddlewares()
    .AddProjectServices(builder.Configuration)
    .AddJwtSecurityServices(builder.Configuration)
    .AddAuthorizationPolicies()
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

app.Run();

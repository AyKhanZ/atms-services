using ATMS.Admin.API.Middleware;
using ATMS.Admin.Service.Modules;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddTransient<ExceptionsMiddleware>();

var sqlConnection = builder.Configuration["Admin:Databases:PgSql"] ?? throw new Exception("PgSql config not found");
var mongoConnection = builder.Configuration["Admin:Databases:Mongo"] ?? throw new Exception("Mongo config not found");

builder.Services.AddAdminServices(sqlConnection ,mongoConnection);

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

app.MapControllers();

app.Run();
    
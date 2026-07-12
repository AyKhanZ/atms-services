using ATMS.Swagger.Middlewares;
using Microsoft.Extensions.DependencyInjection;
using System.IdentityModel.Tokens.Jwt;
using ATMS.Infrastructure.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ATMS.Application.Exceptions.Configuration;
using ATMS.Application.Exceptions.Resources;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi;
using System.Reflection;
using ATMS.Swagger.Constants;

namespace ATMS.Swagger.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
                policy.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod());
        });
        return services;
    }


    public static IServiceCollection AddCustomMiddlewares(this IServiceCollection services)
    {
        services.AddTransient<ExceptionsMiddleware>();
        return services;
    }


    public static IServiceCollection AddJwtSecurityServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var jwtOptions = configuration.GetSection(nameof(JwtOptions)).Get<JwtOptions>()
                                 ?? throw new ConfigurationException(ConfigurationErrorType.JwtSectionNotFound,
                                     string.Format(LogMessages.ConfigSectionNotFound, nameof(JwtOptions)));

                options.RequireHttpsMetadata = true;
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    ClockSkew = TimeSpan.Zero
                };
            });

        return services;
    }


    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services, string title)
    {
        services.AddSwaggerGen(options =>
        {
            var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "ATMS.*.xml");
            foreach (var xmlFile in xmlFiles) {
                options.IncludeXmlComments(xmlFile);
            }
            options.SwaggerDoc(SwaggerConstants.ApiVersion, new OpenApiInfo
            {
                Title = title,
                Description = SwaggerConstants.GetDescription(title),
                Version = SwaggerConstants.ApiVersion,
                Contact = new OpenApiContact
                {
                    Name = "Github",
                    Url = new Uri("https://github.com/AyKhanZ/atms-services"),
                }
            });

            options.AddServer(new OpenApiServer { Url = "/" });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Enter jwt token without 'Bearer'. Example: eyJhbGciOiJIUzI1NiIsInR...",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
            });
            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });
        });

        return services;
    }
}
using System.Net;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ATMS.Application.Realtime;
using ATMS.Project.API.Hubs;
using ATMS.Swagger.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Project.API.Tests;

public sealed class RealtimeHubAuthorizationTest
{
    private const string Key = "realtime-test-signing-key-12345678901234567890";
    private const string Issuer = "realtime-test-issuer";
    private const string Audience = "realtime-test-audience";

    [Fact]
    public async Task QueryToken_AuthenticatesOnlyHubRequests()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.ConfigureKestrel(options => options.Listen(IPAddress.Loopback, 0));
        builder.Configuration["JwtOptions:Key"] = Key;
        builder.Configuration["JwtOptions:Issuer"] = Issuer;
        builder.Configuration["JwtOptions:Audience"] = Audience;
        builder.Services.AddSignalR();
        builder.Services.AddJwtSecurityServices(builder.Configuration);
        builder.Services.AddAuthorizationPolicies();

        await using var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapHub<RealtimeHub>(RealtimeConstants.HubPath);
        app.MapGet("/api/test", () => "ok").RequireAuthorization();
        await app.StartAsync();

        var address = app.Services.GetRequiredService<IServer>()
            .Features.Get<IServerAddressesFeature>()!.Addresses.Single();
        using var client = new HttpClient();
        using var anonymous = await client.PostAsync(
            $"{address}{RealtimeConstants.HubPath}/negotiate?negotiateVersion=1",
            new StringContent(string.Empty));
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);

        var token = CreateToken();
        using var authenticated = await client.PostAsync(
            $"{address}{RealtimeConstants.HubPath}/negotiate?negotiateVersion=1&access_token={Uri.EscapeDataString(token)}",
            new StringContent(string.Empty));
        Assert.Equal(HttpStatusCode.OK, authenticated.StatusCode);

        using var nonHub = await client.GetAsync($"{address}/api/test?access_token={Uri.EscapeDataString(token)}");
        Assert.Equal(HttpStatusCode.Unauthorized, nonHub.StatusCode);
    }

    private static string CreateToken()
    {
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: [new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString())],
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

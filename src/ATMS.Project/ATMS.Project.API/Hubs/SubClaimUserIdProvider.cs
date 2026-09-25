using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace ATMS.Project.API.Hubs;

public sealed class SubClaimUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection) => GetUserId(connection.User);

    public string? GetUserId(ClaimsPrincipal? user) => user?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
}

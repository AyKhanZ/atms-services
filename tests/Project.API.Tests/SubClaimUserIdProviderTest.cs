using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ATMS.Project.API.Hubs;

namespace Project.API.Tests;

public sealed class SubClaimUserIdProviderTest
{
    [Fact]
    public void GetUserId_WithSubClaim_ReturnsSub()
    {
        var expected = Guid.NewGuid().ToString();
        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(JwtRegisteredClaimNames.Sub, expected)]));

        var actual = new SubClaimUserIdProvider().GetUserId(user);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetUserId_WithoutSubClaim_ReturnsNull()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity());

        var actual = new SubClaimUserIdProvider().GetUserId(user);

        Assert.Null(actual);
    }
}

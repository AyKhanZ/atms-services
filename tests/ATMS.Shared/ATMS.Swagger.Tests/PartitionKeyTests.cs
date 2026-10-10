using System.Net;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using ATMS.Swagger.RateLimiting;
using Microsoft.AspNetCore.Http;
using Moq;

namespace ATMS.Swagger.Tests;

public class PartitionKeyTests
{
    [Fact]
    public void Key_WithSub_ReturnsUser()
    {
        var context = new Mock<HttpContext>();
        context.Setup(item => item.User).Returns(new ClaimsPrincipal(
            new ClaimsIdentity([new Claim(JwtRegisteredClaimNames.Sub, "42")])));

        Assert.Equal("user:42", RateLimitPartitions.Key(context.Object));
    }

    [Fact]
    public void Key_WithoutSub_ReturnsIp()
    {
        var connection = new Mock<ConnectionInfo>();
        connection.Setup(item => item.RemoteIpAddress).Returns(IPAddress.Parse("203.0.113.8"));
        var context = new Mock<HttpContext>();
        context.Setup(item => item.User).Returns(new ClaimsPrincipal(new ClaimsIdentity()));
        context.Setup(item => item.Connection).Returns(connection.Object);

        Assert.Equal("ip:203.0.113.8", RateLimitPartitions.Key(context.Object));
    }

    [Fact]
    public void IpKey_IgnoresSub()
    {
        var connection = new Mock<ConnectionInfo>();
        connection.Setup(item => item.RemoteIpAddress).Returns(IPAddress.Parse("203.0.113.8"));
        var context = new Mock<HttpContext>();
        context.Setup(item => item.User).Returns(new ClaimsPrincipal(
            new ClaimsIdentity([new Claim(JwtRegisteredClaimNames.Sub, "42")])));
        context.Setup(item => item.Connection).Returns(connection.Object);

        Assert.Equal("ip:203.0.113.8", RateLimitPartitions.IpKey(context.Object));
    }
}

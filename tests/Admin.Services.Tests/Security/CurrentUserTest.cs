using System.Security.Claims;
using ATMS.Application.Constants;
using ATMS.Application.Infrastructure;
using Microsoft.AspNetCore.Http;

namespace Admin.Services.Tests.Security;

public sealed class CurrentUserTest
{
    [Theory]
    [InlineData("true", true)]
    [InlineData("True", true)]
    [InlineData("false", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void HasCompletedOnboarding_ReadsOnboardingClaim(string? claimValue, bool expected)
    {
        var claims = claimValue is null
            ? []
            : new[] { new Claim(CustomClaimTypes.OnboardingCompleted, claimValue) };
        var accessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims)) }
        };

        var currentUser = new CurrentUser(accessor);

        Assert.Equal(expected, currentUser.HasCompletedOnboarding);
    }

    [Fact]
    public void HasCompletedOnboarding_NoHttpContext_ReturnsFalse()
    {
        var currentUser = new CurrentUser(new HttpContextAccessor());

        Assert.False(currentUser.HasCompletedOnboarding);
    }
}

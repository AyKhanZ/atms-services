using ATMS.Admin.Contracts.Commands.Authentication;
using ATMS.Admin.Service.Resources;
using ATMS.Admin.Service.Validation.Authentication;
using Bogus;

namespace Admin.Services.Tests.Validators.Authentication;

public class RefreshTokenValidatorTest
{
    private readonly Faker _faker = new();
    private readonly RefreshTokenValidator _validator = new();

    private RefreshTokenCommand GetCommand(string? refreshToken = null)
    {
        return new RefreshTokenCommand
        {
            RefreshToken = refreshToken ?? _faker.Random.AlphaNumeric(32)
        };
    }
    
    [Fact]
    public async Task Validate_WhenRefreshTokenIsEmpty_ReturnsFailure()
    {
        var result = await _validator.ValidateAsync(GetCommand(refreshToken: string.Empty));
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == AccountMessages.TokenRequired);
    }
}

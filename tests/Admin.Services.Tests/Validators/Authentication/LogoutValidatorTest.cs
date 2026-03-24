using ATMS.Admin.Contracts.Commands.Authentication;
using ATMS.Admin.Service.Validation.Authentication;
using Bogus;

namespace Admin.Services.Tests.Validators.Authentication;

public class LogoutValidatorTest
{
    private readonly LogoutValidator _validator = new();
    private readonly Faker _faker = new();
    private readonly Guid _userId = Guid.NewGuid();

    private LogoutCommand GetCommand(string? refreshToken = null)
    {
        return new LogoutCommand
        {
            UserId =  _userId,
            RefreshToken = refreshToken ?? _faker.Random.AlphaNumeric(32)
        };
    }

    [Fact]
    public async Task Validate_WithValidCommand_ReturnsSuccess()
    {
        var command = GetCommand();
 
        var result = await _validator.ValidateAsync(command);
 
        Assert.True(result.IsValid);
    }
 
    [Fact]
    public async Task Validate_WhenRefreshTokenIsEmpty_ReturnsFailure()
    {
        var command = GetCommand(refreshToken: string.Empty);
 
        var result = await _validator.ValidateAsync(command);
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "RefreshToken is required .");
    }
}

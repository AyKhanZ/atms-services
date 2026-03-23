
using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Service.Validation.Account;
using Bogus;

namespace Admin.Services.Tests.Validators.Account;

public class ConfirmEmailValidatorTest
{
    private readonly Faker _faker = new();
    private readonly ConfirmEmailValidator _validator = new();

    private ConfirmEmailCommand GetCommand(string? token = null)
    {
        return new ConfirmEmailCommand
        {
            Token = token ?? _faker.Random.String(500),
        };
    }
    
    [Fact]
    public async Task Validate_Success()
    {
        var result = await _validator.ValidateAsync(GetCommand());
 
        Assert.True(result.IsValid);
    }
    
    [Theory]
    [InlineData("    ")]
    [InlineData("")]
    public async Task Validate_TokenIsNullOrEmpty_Fail(string? token)
    {
        var result = await _validator.ValidateAsync(GetCommand(token));
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Token is required");
    }
}

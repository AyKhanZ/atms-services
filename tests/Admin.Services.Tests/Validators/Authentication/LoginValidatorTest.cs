using ATMS.Admin.Contracts.Commands.Authentication;
using ATMS.Admin.Service.Validation.Authentication;
using Bogus;

namespace Admin.Services.Tests.Validators.Authentication;

public class LoginValidatorTest
{
    private readonly Faker _faker = new();
    private readonly LoginValidator _validator = new();

    private LoginCommand GetCommand(string? email = null, string? password = null)
    {
        return new LoginCommand
        {
            Email = email ?? _faker.Internet.Email(),
            Password = password ?? _faker.Internet.Password()
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
    public async Task Validate_WhenEmailIsEmpty_ReturnsFailure()
    {
        var command = GetCommand(email: string.Empty);
 
        var result = await _validator.ValidateAsync(command);
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Email is required .");
    }
 
    
    [Fact]
    public async Task Validate_WhenPasswordIsEmpty_ReturnsFailure()
    {
        var command = GetCommand(password: string.Empty);
 
        var result = await _validator.ValidateAsync(command);
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Password is required .");
    }
}

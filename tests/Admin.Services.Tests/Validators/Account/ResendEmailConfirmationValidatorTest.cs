using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Service.Resources;
using ATMS.Admin.Service.Validation.Account;
using ATMS.Application.Exceptions.Resources;
using Bogus;

namespace Admin.Services.Tests.Validators.Account;

public class ResendEmailConfirmationValidatorTest
{
    private readonly Faker _faker = new();
    private readonly ResendEmailConfirmationValidator _validator = new();

    private ResendEmailConfirmationCommand GetCommand(string? email = null) =>
        new() { Email = email ?? _faker.Internet.Email() };
 
    [Fact]
    public async Task Validate_WithValidCommand_ReturnsSuccess()
    {
        var result = await _validator.ValidateAsync(GetCommand());
 
        Assert.True(result.IsValid);
    }
 
    [Fact]
    public async Task Validate_WhenEmailIsEmpty_ReturnsFailure()
    {
        var result = await _validator.ValidateAsync(GetCommand(email: string.Empty));
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == AccountMessages.EmailRequired);
    }
 
    [Fact]
    public async Task Validate_WhenEmailIsInvalid_ReturnsFailure()
    {
        var result = await _validator.ValidateAsync(GetCommand(email: "not-an-email"));
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == ValidationMessages.InvalidEmailFormat);
    }
}

using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Service.Resources;
using ATMS.Admin.Service.Validation.Account;
using ATMS.Application.Exceptions.Resources;
using Bogus;

namespace Admin.Services.Tests.Validators.Account;

public class ChangePasswordValidatorTest
{
    private readonly Faker _faker = new();
    private readonly ChangePasswordValidator _validator = new();

    private ChangePasswordCommand GetCommand(
        string? email = null,
        string? oldPassword = null,
        string? newPassword = null) =>
        new()
        {
            Email = email ?? _faker.Internet.Email(),
            OldPassword = oldPassword ?? "OldPass1!",
            NewPassword = newPassword ?? "NewPass1!"
        };
    
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
        Assert.Contains(result.Errors,
            e => e.ErrorMessage == AccountMessages.EmailRequired);
    }
 
    [Fact]
    public async Task Validate_WhenEmailIsInvalid_ReturnsFailure()
    {
        var result = await _validator.ValidateAsync(GetCommand(email: "not-an-email"));
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == ValidationMessages.InvalidEmailFormat);
    }
    
    
    [Fact]
    public async Task Validate_WhenOldPasswordIsEmpty_ReturnsFailure()
    {
        var result = await _validator.ValidateAsync(GetCommand(oldPassword: string.Empty));
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == AccountMessages.OldPasswordRequired);
    }
 
 
    [Fact]
    public async Task Validate_WhenNewPasswordIsEmpty_ReturnsFailure()
    {
        var result = await _validator.ValidateAsync(GetCommand(newPassword: string.Empty));
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == AccountMessages.NewPasswordRequired);
    }
 
    [Fact]
    public async Task Validate_WhenNewPasswordTooShort_ReturnsFailure()
    {
        var result = await _validator.ValidateAsync(GetCommand(newPassword: "A1!bc"));
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == string.Format(AccountMessages.PasswordTooShort, 6));
    }
 
    [Fact]
    public async Task Validate_WhenNewPasswordTooLong_ReturnsFailure()
    {
        var result = await _validator.ValidateAsync(GetCommand(newPassword: $"A1!{"a".PadRight(41, 'a')}"));
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == string.Format(AccountMessages.PasswordTooLong, 40));
    }
 
    [Theory]
    [InlineData("newpassword1!")]  // no uppercase
    [InlineData("NewPassword1")]   // no special char
    [InlineData("NewPassword!")]   // no number
    [InlineData("New Pass1!")]     // space
    public async Task Validate_WhenNewPasswordInvalid_ReturnsFailure(string password)
    {
        var result = await _validator.ValidateAsync(GetCommand(newPassword: password));
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == AccountMessages.PasswordInvalidFormat);
    }
}

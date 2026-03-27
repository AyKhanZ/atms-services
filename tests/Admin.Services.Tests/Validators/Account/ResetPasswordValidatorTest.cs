using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Service.Resources;
using ATMS.Admin.Service.Validation.Account;
using ATMS.Application.Exceptions.Resources;

namespace Admin.Services.Tests.Validators.Account;

public class ResetPasswordValidatorTest
{
    private readonly ResetPasswordValidator _validator = new();
 
    private const string ValidPassword = "ValidPass1!";
    private const string ValidToken = "valid-token";

    private ResetPasswordCommand GetCommand(
        string? password = null,
        string? confirmPassword = null,
        string? token = null)
    {
        var pass = password ?? ValidPassword;
        return new ResetPasswordCommand
        {
            Password = pass,
            ConfirmPassword = confirmPassword ?? pass,
            Token = token ?? ValidToken
        };
    }
 
    [Fact]
    public async Task Validate_WithValidCommand_ReturnsSuccess()
    {
        var result = await _validator.ValidateAsync(GetCommand());
 
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WhenPasswordIsEmpty_ReturnsFailure()
    {
        var result = await _validator.ValidateAsync(GetCommand(password: string.Empty, confirmPassword: string.Empty));
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == AccountMessages.PasswordRequired);
    }
 
    [Fact]
    public async Task Validate_WhenPasswordTooShort_ReturnsFailure()
    {
        var result = await _validator.ValidateAsync(GetCommand(password: "A1!bc", confirmPassword: "A1!bc"));
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == string.Format(AccountMessages.PasswordTooShort, 6));
    }
 
    [Fact]
    public async Task Validate_WhenPasswordTooLong_ReturnsFailure()
    {
        var pass = $"A1!{"a".PadRight(41, 'a')}";
        var result = await _validator.ValidateAsync(GetCommand(password: pass, confirmPassword: pass));
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == string.Format(AccountMessages.PasswordTooLong, 40));
    }
 
    [Theory]
    [InlineData("newpassword1!")]  // no uppercase
    [InlineData("NewPassword1")]   // no special char
    [InlineData("NewPassword!")]   // no number
    [InlineData("New Pass1!")]     // space
    public async Task Validate_WhenPasswordInvalid_ReturnsFailure(string password)
    {
        var result = await _validator.ValidateAsync(GetCommand(password: password, confirmPassword: password));
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == AccountMessages.PasswordInvalidFormat);
    }
 
 
    [Fact]
    public async Task Validate_WhenConfirmPasswordIsEmpty_ReturnsFailure()
    {
        var result = await _validator.ValidateAsync(GetCommand(confirmPassword: string.Empty));
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == AccountMessages.ConfirmPasswordRequired);
    }
 
    [Fact]
    public async Task Validate_WhenPasswordsDoNotMatch_ReturnsFailure()
    {
        var result = await _validator.ValidateAsync(GetCommand(confirmPassword: "OtherPass1!"));
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == AccountMessages.PasswordsNotMatches);
    }
 
 
    [Fact]
    public async Task Validate_WhenTokenIsEmpty_ReturnsFailure()
    {
        var result = await _validator.ValidateAsync(GetCommand(token: string.Empty));
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == AccountMessages.TokenRequired);
    }
}

using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Service.Validation.Account;

namespace Admin.Services.Tests.Validators.Account;

public class ChangePasswordValidatorTest
{
    private readonly ChangePasswordValidator _validator = new();

    [Theory]
    [InlineData("", "ValidPass1!", "ValidPass1!", "OldPassword")]
    [InlineData("OldPass1!", "Short1!", "Short1!", "NewPassword")]
    [InlineData("OldPass1!", "OldPass1!", "OldPass1!", "NewPassword")]
    [InlineData("OldPass1!", "ValidPass1!", "Different1!", "ConfirmPassword")]
    public async Task Validate_InvalidPasswordFields_ReturnsFieldError(
        string oldPassword, string newPassword, string confirmation, string field)
    {
        var result = await _validator.ValidateAsync(new ChangePasswordCommand
        {
            OldPassword = oldPassword,
            NewPassword = newPassword,
            ConfirmPassword = confirmation
        });

        Assert.Contains(result.Errors, error => error.PropertyName == field);
    }

    [Fact]
    public async Task Validate_ValidPasswords_Succeeds()
    {
        var result = await _validator.ValidateAsync(new ChangePasswordCommand
        {
            OldPassword = "OldPass123!",
            NewPassword = "NewPass123!",
            ConfirmPassword = "NewPass123!"
        });

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("newpassword1!")]
    [InlineData("NewPassword1")]
    [InlineData("NewPassword!")]
    [InlineData("New Pass1!")]
    [InlineData("NEWPASSWORD1!")]
    public async Task Validate_InvalidNewPassword_ReturnsNewPasswordError(string password)
    {
        var result = await _validator.ValidateAsync(new ChangePasswordCommand
        {
            OldPassword = "OldPass123!",
            NewPassword = password,
            ConfirmPassword = password
        });

        Assert.Contains(result.Errors, error => error.PropertyName == "NewPassword");
    }

    [Fact]
    public async Task Validate_OverFortyCharacters_ReturnsNewPasswordError()
    {
        var password = "LongPass1!" + new string('a', 31);
        var result = await _validator.ValidateAsync(new ChangePasswordCommand
        {
            OldPassword = "OldPass123!",
            NewPassword = password,
            ConfirmPassword = password
        });

        Assert.Contains(result.Errors, error => error.PropertyName == "NewPassword");
    }
}

using System.Linq.Expressions;
using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Data.Entities.Tokens;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Validation.Account;
using Moq;

namespace Admin.Services.Tests.Validators.Account;

public class ResetPasswordValidatorTest
{
    private readonly Mock<IPasswordResetTokenRepository> _tokenRepositoryMock;
    private readonly ResetPasswordValidator _validator;
 
    private const string ValidPassword = "ValidPass1!";
    private const string ValidToken = "valid-token";
 
    public ResetPasswordValidatorTest()
    {
        _tokenRepositoryMock = new Mock<IPasswordResetTokenRepository>();
        _validator = new ResetPasswordValidator(_tokenRepositoryMock.Object);
 
        SetupTokenExists(true);
        SetupTokenEntity(DateTime.UtcNow.AddHours(1));
    }
 
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
 
    private void SetupTokenExists(bool exists) =>
        _tokenRepositoryMock
            .Setup(r => r.IsExistAsync(It.IsAny<string>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);
 
    private void SetupTokenEntity(DateTime expiresAt) =>
        _tokenRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<PasswordResetToken, bool>>>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PasswordResetToken { Token = ValidToken, ExpiresAt = expiresAt });
 
 
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
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Password is required.");
    }
 
    [Fact]
    public async Task Validate_WhenPasswordTooShort_ReturnsFailure()
    {
        var result = await _validator.ValidateAsync(GetCommand(password: "A1!bc", confirmPassword: "A1!bc"));
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Password must be at least 6 symbols");
    }
 
    [Fact]
    public async Task Validate_WhenPasswordTooLong_ReturnsFailure()
    {
        var pass = $"A1!{"a".PadRight(41, 'a')}";
        var result = await _validator.ValidateAsync(GetCommand(password: pass, confirmPassword: pass));
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Password must be less than 40 symbols");
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
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Password must include uppercase, number, special char (!@#$%^&*()-_=+), no spaces");
    }
 
 
    [Fact]
    public async Task Validate_WhenConfirmPasswordIsEmpty_ReturnsFailure()
    {
        var result = await _validator.ValidateAsync(GetCommand(confirmPassword: string.Empty));
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "ConfirmPassword is required.");
    }
 
    [Fact]
    public async Task Validate_WhenPasswordsDoNotMatch_ReturnsFailure()
    {
        var result = await _validator.ValidateAsync(GetCommand(confirmPassword: "OtherPass1!"));
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Password and confirmation password do not match.");
    }
 
 
    [Fact]
    public async Task Validate_WhenTokenIsEmpty_ReturnsFailure()
    {
        var result = await _validator.ValidateAsync(GetCommand(token: string.Empty));
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Reset password token is required.");
    }
 
    [Fact]
    public async Task Validate_WhenTokenNotFound_ReturnsFailure()
    {
        SetupTokenExists(false);
 
        var result = await _validator.ValidateAsync(GetCommand());
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Invalid password reset token.");
    }
 
    [Fact]
    public async Task Validate_WhenTokenExpired_ReturnsFailure()
    {
        SetupTokenEntity(DateTime.UtcNow.AddHours(-1));
 
        var result = await _validator.ValidateAsync(GetCommand());
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Expired password reset token.");
    }
}

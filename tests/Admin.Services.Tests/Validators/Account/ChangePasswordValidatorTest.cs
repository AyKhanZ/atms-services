using System.Linq.Expressions;
using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Validation.Account;
using Bogus;
using Moq;

namespace Admin.Services.Tests.Validators.Account;

public class ChangePasswordValidatorTest
{
    private readonly Faker _faker;
    private readonly ChangePasswordValidator _validator;
    private readonly Mock<IUserRepository> _userRepositoryMock;
 
    public ChangePasswordValidatorTest()
    {
        _faker = new Faker();
        _userRepositoryMock = new Mock<IUserRepository>();
        _validator = new ChangePasswordValidator(_userRepositoryMock.Object);
        
        SetupUserExists(true);
    }
    
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
    
    private void SetupUserExists(bool exists) =>
        _userRepositoryMock
            .Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<User, bool>>>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);
    
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
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Email is required");
    }
 
    [Fact]
    public async Task Validate_WhenEmailIsInvalid_ReturnsFailure()
    {
        var result = await _validator.ValidateAsync(GetCommand(email: "not-an-email"));
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Please enter a valid email (e.g. user@example.com)");
    }
 
    [Fact]
    public async Task Validate_WhenUserNotFound_ReturnsFailure()
    {
        SetupUserExists(false);
 
        var result = await _validator.ValidateAsync(GetCommand());
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "No account found with this email");
    }
    
    
    [Fact]
    public async Task Validate_WhenOldPasswordIsEmpty_ReturnsFailure()
    {
        var result = await _validator.ValidateAsync(GetCommand(oldPassword: string.Empty));
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Old password is required");
    }
 
 
    [Fact]
    public async Task Validate_WhenNewPasswordIsEmpty_ReturnsFailure()
    {
        var result = await _validator.ValidateAsync(GetCommand(newPassword: string.Empty));
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "New password is required");
    }
 
    [Fact]
    public async Task Validate_WhenNewPasswordTooShort_ReturnsFailure()
    {
        var result = await _validator.ValidateAsync(GetCommand(newPassword: "A1!bc"));
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Password must be at least 6 symbols");
    }
 
    [Fact]
    public async Task Validate_WhenNewPasswordTooLong_ReturnsFailure()
    {
        var result = await _validator.ValidateAsync(GetCommand(newPassword: $"A1!{"a".PadRight(41, 'a')}"));
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Password must be less than 40 symbols");
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
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Password must include uppercase, number, special char (!@#$%^&*()-_=+), no spaces");
    }
}

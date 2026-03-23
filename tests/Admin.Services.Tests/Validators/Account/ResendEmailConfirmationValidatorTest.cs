using System.Linq.Expressions;
using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Validation.Account;
using Bogus;
using Moq;

namespace Admin.Services.Tests.Validators.Account;

public class ResendEmailConfirmationValidatorTest
{
    private readonly Faker _faker;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly ResendEmailConfirmationValidator _validator;
 
    public ResendEmailConfirmationValidatorTest()
    {
        _faker = new Faker();
        _userRepositoryMock = new Mock<IUserRepository>();
        _validator = new ResendEmailConfirmationValidator(_userRepositoryMock.Object);
 
        SetupIsExist(true);
    }
 
    private ResendEmailConfirmationCommand GetCommand(string? email = null) =>
        new() { Email = email ?? _faker.Internet.Email() };
 
    private void SetupIsExist(bool exists) =>
        _userRepositoryMock
            .Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
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
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Email is required.");
    }
 
    [Fact]
    public async Task Validate_WhenEmailIsInvalid_ReturnsFailure()
    {
        var result = await _validator.ValidateAsync(GetCommand(email: "not-an-email"));
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Invalid email format.");
    }
 
    [Fact]
    public async Task Validate_WhenUserNotFound_ReturnsFailure()
    {
        _userRepositoryMock
            .SetupSequence(r => r.IsExistAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
 
        var result = await _validator.ValidateAsync(GetCommand());
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "User with the specified email does not exist.");
    }
 
    [Fact]
    public async Task Validate_WhenEmailAlreadyConfirmed_ReturnsFailure()
    {
        _userRepositoryMock
            .SetupSequence(r => r.IsExistAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)   // IsUserExistAsync
            .ReturnsAsync(false); // IsEmailConfirmedAsync
        
        var result = await _validator.ValidateAsync(GetCommand());
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Email is already confirmed.");
    }
}

using System.Linq.Expressions;
using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Validation.Account;
using Bogus;
using Moq;

namespace Admin.Services.Tests.Validators.Account;

public class ForgotPasswordValidatorTest
{
    private readonly Faker _faker;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly ForgotPasswordValidator _validator;

    public ForgotPasswordValidatorTest()
    {
        _faker = new Faker();
        _userRepositoryMock = new Mock<IUserRepository>();
        _validator = new ForgotPasswordValidator(_userRepositoryMock.Object);

        SetupUserExists(true);
    }
    
    private ForgotPasswordCommand GetCommand(string? email = null)
    {
        return new ForgotPasswordCommand
        {
            Email = email ?? _faker.Internet.Email(),
        };
    }
    
    private void SetupUserExists(bool exists) =>
        _userRepositoryMock
            .Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<User, bool>>>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);

    [Fact]
    public async Task Validate_Success()
    {
        var command = GetCommand();
        
        var result = await _validator.ValidateAsync(command, CancellationToken.None);
        
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
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Email is invalid");
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
    public async Task Validate_WhenEmailIsEmpty_DoesNotCheckExistence()
    {
        await _validator.ValidateAsync(GetCommand(email: string.Empty));
 
        _userRepositoryMock.Verify(
            r => r.IsExistAsync(It.IsAny<Expression<Func<User, bool>>>(), 
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}

using System.Linq.Expressions;
using ATMS.Admin.Contracts.Commands.Authentication;
using ATMS.Admin.Contracts.Enums;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Validation.Authentication;
using Bogus;
using Moq;

namespace Admin.Services.Tests.Validators.Authentication;

public class LoginValidatorTest
{
    private readonly Faker _faker;
    private readonly LoginValidator _validator;
    private readonly Mock<IUserRepository> _userRepositoryMock;
 
    public LoginValidatorTest()
    {
        _faker = new Faker();
        _userRepositoryMock = new Mock<IUserRepository>();
        _validator = new LoginValidator(_userRepositoryMock.Object);
    }

    private LoginCommand GetCommand(string? email = null, string? password = null)
    {
        return new LoginCommand
        {
            Email = email ?? _faker.Internet.Email(),
            Password = password ?? _faker.Internet.Password()
        };
    }
 
    private void SetupUser(string email, User? user) =>
        _userRepositoryMock
            .Setup(r => r.FindAsync(It.Is<Expression<Func<User, bool>>>(e => true), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
 
    private void SetupEmailExists(bool exists) =>
        _userRepositoryMock
            .Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);
 
    private User CreateActiveUser(string email) =>
        new()
        {
            Id = Guid.NewGuid(),
            Email = email,
            Name = _faker.Name.FirstName(),
            Surname = _faker.Name.LastName(),
            EmailConfirmed = true,
            UserStatusId = (int)UserStatusEnum.Active
        };
    
    
    [Fact]
    public async Task Validate_WithValidCommand_ReturnsSuccess()
    {
        var command = GetCommand();
        var user = CreateActiveUser(command.Email);
 
        SetupUser(command.Email, user);
        SetupEmailExists(true);
 
        var result = await _validator.ValidateAsync(command);
 
        Assert.True(result.IsValid);
    }

    
    [Fact]
    public async Task Validate_WhenEmailIsEmpty_ReturnsFailure()
    {
        var command = GetCommand(email: string.Empty);
        SetupUser(command.Email, null);
        SetupEmailExists(false);
 
        var result = await _validator.ValidateAsync(command);
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Email is required .");
    }
 
    [Fact]
    public async Task Validate_WhenEmailNotFound_ReturnsFailure()
    {
        var command = GetCommand();
        SetupUser(command.Email, null);
        SetupEmailExists(false);
 
        var result = await _validator.ValidateAsync(command);
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "User with such email doesn't exist .");
    }
 
    [Fact]
    public async Task Validate_WhenEmailNotConfirmed_ReturnsFailure()
    {
        var command = GetCommand();
        var user = CreateActiveUser(command.Email);
        user.EmailConfirmed = false;
 
        SetupUser(command.Email, user);
        SetupEmailExists(true);
 
        var result = await _validator.ValidateAsync(command);
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Email not confirmed .");
    }

    
    [Fact]
    public async Task Validate_WhenPasswordIsEmpty_ReturnsFailure()
    {
        var command = GetCommand(password: string.Empty);
        var user = CreateActiveUser(command.Email);
 
        SetupUser(command.Email, user);
        SetupEmailExists(true);
 
        var result = await _validator.ValidateAsync(command);
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Password is required .");
    }
    
 
    [Fact]
    public async Task Validate_WhenUserIsDeleted_ReturnsFailure()
    {
        var command = GetCommand();
        var user = CreateActiveUser(command.Email);
        user.UserStatusId = (int)UserStatusEnum.Inactive;
 
        SetupUser(command.Email, user);
        SetupEmailExists(true);
 
        var result = await _validator.ValidateAsync(command);
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Your account is not active anymore. Please, contact support .");
    }
 
    [Fact]
    public async Task Validate_WhenUserIsLocked_ReturnsFailure()
    {
        var command = GetCommand();
        var user = CreateActiveUser(command.Email);
        user.UserStatusId = (int)UserStatusEnum.Locked;
        user.LockoutEnd = DateTime.UtcNow.AddMinutes(10);
 
        SetupUser(command.Email, user);
        SetupEmailExists(true);
 
        var result = await _validator.ValidateAsync(command);
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.StartsWith("Account is locked."));
    }
}

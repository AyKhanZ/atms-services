using System.Linq.Expressions;
using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Validation.Account;
using Bogus;
using Moq;

namespace Admin.Services.Tests.Validators.Account;

public class RegisterValidatorTest
{
    private readonly Faker _faker;
    private readonly Mock<IUserRepository> _userRepository;
    private readonly Mock<IRoleRepository> _roleRepository;
    private readonly RegisterUserValidator _validator;

    public RegisterValidatorTest()
    {
        _faker =  new Faker();
        _userRepository = new Mock<IUserRepository>();
        _roleRepository = new Mock<IRoleRepository>();
        
        _validator =  new RegisterUserValidator(_userRepository.Object, _roleRepository.Object);
        
        SetupEmailUnique(true);
        SetupRoleExists(true);
    }

    private RegisterCommand CreateCommand(
        string? email = null,
        string? name = null,
        string? surname = null,
        Guid? roleId = null )
    {
        return new RegisterCommand
        {
            Email = email ?? _faker.Internet.Email(),
            Name = name ?? _faker.Name.FirstName(),
            Surname = surname ??  _faker.Name.LastName(),
            RoleId = roleId ?? Guid.NewGuid(),
        };
    }
    
    private void SetupEmailUnique(bool isUnique) =>
        _userRepository
            .Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(!isUnique);
 
    private void SetupRoleExists(bool exists) =>
        _roleRepository
            .Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<Role, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);
    
    [Fact]
    public async Task Validate_WithValidCommand_ReturnsSuccess()
    {
        var result = await _validator.ValidateAsync(CreateCommand());
 
        Assert.True(result.IsValid);
    }
    
    [Fact]
    public async Task Validate_WhenNameIsEmpty_ReturnsFailure()
    {
        var result = await _validator.ValidateAsync(CreateCommand(name: string.Empty));
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Name is required .");
    }
 
    [Fact]
    public async Task Validate_WhenNameExceedsMaxLength_ReturnsFailure()
    {
        var result = await _validator.ValidateAsync(CreateCommand(name: _faker.Random.String(51)));
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Name should be max 50 symbols .");
    }
    
    [Fact]
    public async Task Validate_WhenSurnameIsEmpty_ReturnsFailure()
    {
        var result = await _validator.ValidateAsync(CreateCommand(surname: string.Empty));
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Surname is required .");
    }
 
    [Fact]
    public async Task Validate_WhenSurnameExceedsMaxLength_ReturnsFailure()
    {
        var result = await _validator.ValidateAsync(CreateCommand(surname: _faker.Random.String(101)));
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Surname should be max 100 symbols .");
    }
    
    
    [Fact]
    public async Task Validate_WhenEmailIsEmpty_ReturnsFailure()
    {
        var result = await _validator.ValidateAsync(CreateCommand(email: string.Empty));
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Email is required .");
    }
 
    [Fact]
    public async Task Validate_WhenEmailAlreadyExists_ReturnsFailure()
    {
        SetupEmailUnique(false);
 
        var result = await _validator.ValidateAsync(CreateCommand());
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "User with this email already exist .");
    }
    
    [Fact]
    public async Task Validate_WhenRoleNotFound_ReturnsFailure()
    {
        SetupRoleExists(false);
 
        var result = await _validator.ValidateAsync(CreateCommand());
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Role doesn't exist .");
    }
}

using System.Linq.Expressions;
using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Resources;
using ATMS.Admin.Service.Validation.Account;
using ATMS.Application.Exceptions.Resources;
using Bogus;
using Moq;

namespace Admin.Services.Tests.Validators.Account;

public class RegisterValidatorTest
{
    private readonly Faker _faker;
    private readonly Mock<IUserRepository> _userRepository;
    private readonly RegisterUserValidator _validator;

    public RegisterValidatorTest()
    {
        _faker =  new Faker();
        _userRepository = new Mock<IUserRepository>();
        
        _validator =  new RegisterUserValidator(_userRepository.Object);
        
        SetupEmailUnique(true);
    }

    private RegisterCommand CreateCommand(
        string? email = null,
        string? name = null,
        string? surname = null,
        Guid? roleId = null)
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
            .Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(!isUnique);
 
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
        Assert.Contains(result.Errors, e => e.ErrorMessage == AccountMessages.NameRequired);
    }
 
    [Fact]
    public async Task Validate_WhenNameExceedsMaxLength_ReturnsFailure()
    {
        var result = await _validator.ValidateAsync(CreateCommand(name: _faker.Random.String(51)));
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == string.Format(AccountMessages.NameShouldBeLessThan, 50));
    }
    
    [Fact]
    public async Task Validate_WhenSurnameIsEmpty_ReturnsFailure()
    {
        var result = await _validator.ValidateAsync(CreateCommand(surname: string.Empty));
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == AccountMessages.SurnameRequired);
    }
 
    [Fact]
    public async Task Validate_WhenSurnameExceedsMaxLength_ReturnsFailure()
    {
        var result = await _validator.ValidateAsync(CreateCommand(surname: _faker.Random.String(101)));
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == string.Format(AccountMessages.SurnameShouldBeLessThan, 100));
    }
    
    
    [Fact]
    public async Task Validate_WhenEmailIsEmpty_ReturnsFailure()
    {
        var result = await _validator.ValidateAsync(CreateCommand(email: string.Empty));
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == AccountMessages.EmailRequired);
    }
 
    [Fact]
    public async Task Validate_WhenEmailAlreadyExists_ReturnsFailure()
    {
        SetupEmailUnique(false);
 
        var result = await _validator.ValidateAsync(CreateCommand());
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == AccountMessages.UserAlreadyExists);
    }
    
    [Fact]
    public async Task Validate_WhenRoleIdIsEmpty_ReturnsFailure()
    {
        var result = await _validator.ValidateAsync(CreateCommand(roleId: Guid.Empty));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors,
            e => e.ErrorMessage == ValidationMessages.RoleIdRequired);
    }
}

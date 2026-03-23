using System.Linq.Expressions;
using ATMS.Admin.Contracts.Commands.Role;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Validation.Roles;
using Bogus;
using Moq;

namespace Admin.Services.Tests.Validators.Roles;

public class CreateRoleValidatorTest
{
    private readonly Faker _faker;
    private readonly CreateRoleValidator _validator;
    private readonly Mock<IRoleRepository> _roleRepositoryMock;

    public CreateRoleValidatorTest()
    {
        _roleRepositoryMock =  new Mock<IRoleRepository>();
        _faker =  new Faker();
        _validator = new CreateRoleValidator(_roleRepositoryMock.Object);
        
        _roleRepositoryMock
            .Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<Role, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
    }
    
    private CreateRoleCommand GetCommand(string? name = null, string? desc = null)
    {
        return new CreateRoleCommand {
            Name = name ?? _faker.Random.AlphaNumeric(10),
            Description = desc ?? _faker.Random.AlphaNumeric(10)
        };
    }
    
    [Fact]
    public async Task Validate_WithValidCommand_ReturnsSuccess()
    {
        var command = GetCommand();
 
        var result = await _validator.ValidateAsync(command);
 
        Assert.True(result.IsValid);
    }
    
    [Fact]
    public async Task Validate_WhenNameIsEmpty_ReturnsFailure()
    {
        var command = GetCommand("");
 
        var result = await _validator.ValidateAsync(command);
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Role name is required.");
    }
 
    [Fact]
    public async Task Validate_WhenNameExceedsMaxLength_ReturnsFailure()
    {
        var command = GetCommand(_faker.Random.AlphaNumeric(21));
 
        var result = await _validator.ValidateAsync(command);
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Role name must not exceed 20 characters.");
    }
 
    [Fact]
    public async Task Validate_WhenDescriptionExceedsMaxLength_ReturnsFailure()
    {
        var command = GetCommand(_faker.Random.AlphaNumeric(10), _faker.Random.AlphaNumeric(21));
 
        var result = await _validator.ValidateAsync(command);
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Role description must not exceed 20 characters.");
    }
 
    [Fact]
    public async Task Validate_WhenNameIsEmpty_DoesNotValidateMaxLength()
    {
        var command = GetCommand(string.Empty);
 
        var result = await _validator.ValidateAsync(command);
 
        Assert.Single(result.Errors);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Role name is required.");
    }
    
    [Fact]
    public async Task Validate_WhenRoleNameAlreadyExists_ReturnsFailure()
    {
        var command = GetCommand();
 
        _roleRepositoryMock
            .Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<Role, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
 
        var result = await _validator.ValidateAsync(command);
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Role with this name already exists.");
    }
}

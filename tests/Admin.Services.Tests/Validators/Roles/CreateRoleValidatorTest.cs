using System.Linq.Expressions;
using ATMS.Admin.Contracts.Commands.Role;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Resources;
using ATMS.Admin.Service.Validation.Roles;
using ATMS.Application.Exceptions.Resources;
using Bogus;
using Moq;

namespace Admin.Services.Tests.Validators.Roles;

public class CreateRoleValidatorTest
{
    private readonly Faker _faker;
    private readonly CreateRoleValidator _validator;
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IPermissionRepository> _permissionRepositoryMock;

    public CreateRoleValidatorTest()
    {
        _roleRepositoryMock =  new Mock<IRoleRepository>();
        _permissionRepositoryMock =  new Mock<IPermissionRepository>();
        _faker =  new Faker();
        _validator = new CreateRoleValidator(_roleRepositoryMock.Object, _permissionRepositoryMock.Object);
        
        _roleRepositoryMock
            .Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        
        _permissionRepositoryMock
            .Setup(r => r.GetExistingIdsAsync(It.IsAny<int[]>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([1, 2, 3]);
    }
    
    private CreateRoleCommand GetCommand(string? name = null, string? desc = null, int[]? permissions = null)
    {
        return new CreateRoleCommand {
            Name = name ?? _faker.Random.AlphaNumeric(10),
            Description = desc ?? _faker.Random.AlphaNumeric(10),
            PermissionIds = permissions ?? [1, 2, 3]
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
        Assert.Contains(result.Errors,
            e => e.ErrorMessage == ValidationMessages.NameRequired);
    }
 
    [Fact]
    public async Task Validate_WhenNameExceedsMaxLength_ReturnsFailure()
    {
        var command = GetCommand(_faker.Random.AlphaNumeric(31));
 
        var result = await _validator.ValidateAsync(command);
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors,
            e => e.ErrorMessage == string.Format(ValidationMessages.NameShouldBeLessThan, 30));
    }
 
    [Fact]
    public async Task Validate_WhenDescriptionExceedsMaxLength_ReturnsFailure()
    {
        var command = GetCommand(_faker.Random.AlphaNumeric(10), _faker.Random.AlphaNumeric(101));
 
        var result = await _validator.ValidateAsync(command);
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors,
            e => e.ErrorMessage == string.Format(ValidationMessages.DescriptionShouldBeLessThan, 100));
    }
    
    [Fact]
    public async Task Validate_WhenRoleNameAlreadyExists_ReturnsFailure()
    {
        var command = GetCommand();
 
        _roleRepositoryMock
            .Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
 
        var result = await _validator.ValidateAsync(command);
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors,
            e => e.ErrorMessage == RoleMessages.AlreadyExists);
    }
    
    [Fact]
    public async Task Validate_WhenPermissionIdsEmpty_ReturnsFailure()
    {
        var command = GetCommand(permissions: []);
        
        var resultEmpty = await _validator.ValidateAsync(command);
        
        Assert.False(resultEmpty.IsValid);
        Assert.Contains(resultEmpty.Errors,
            e => e.ErrorMessage == RoleMessages.PermissionsRequired);
    }

    [Fact]
    public async Task Validate_WhenSomePermissionsDoNotExist_ReturnsFailure()
    {
        var command = GetCommand(permissions: [1, 2, 99]);

        _permissionRepositoryMock
            .Setup(r => r.GetExistingIdsAsync(It.IsAny<int[]>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([1, 2]);

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors,
            e => e.ErrorMessage == RoleMessages.PermissionsNotFound);
    }
}

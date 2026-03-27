using System.Linq.Expressions;
using ATMS.Admin.Contracts.Commands.Role;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Validation.Roles;
using Bogus;
using Moq;

namespace Admin.Services.Tests.Validators.Roles;

public class UpdateRoleValidatorTest
{
    private readonly Faker _faker;
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IPermissionRepository> _permissionRepositoryMock;
    private readonly UpdateRoleValidator _validator;

    public UpdateRoleValidatorTest()
    {
        _faker = new Faker();
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _permissionRepositoryMock = new Mock<IPermissionRepository>();
        _validator = new UpdateRoleValidator(_roleRepositoryMock.Object, _permissionRepositoryMock.Object);
        
        _roleRepositoryMock
            .Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<Role, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        
        _permissionRepositoryMock
            .Setup(r => r.GetExistingIdsAsync(It.IsAny<int[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([1, 2, 3]);
    }
    
    private UpdateRoleCommand GetCommand(string? name = null, string? desc = null, int[]? permissionIds = null)
    {
        return new UpdateRoleCommand {
            Id = Guid.NewGuid(),
            Name = name ?? _faker.Random.AlphaNumeric(10),
            Description = desc ?? _faker.Random.AlphaNumeric(10),
            PermissionIds = permissionIds ?? [1, 2, 3]
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
    public async Task Validate_WhenRoleNameAlreadyExists_ReturnsFailure()
    {
        _roleRepositoryMock
            .Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<Role, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = GetCommand();
        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("Role with this name already exists"));
    }

    [Fact]
    public async Task Validate_WhenNameIsEmpty_ReturnsFailure()
    {
        var command = GetCommand(name: "");
        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("Role name is required"));
    }

    [Fact]
    public async Task Validate_WhenNameTooLong_ReturnsFailure()
    {
        var command = GetCommand(name: new string('a', 21));
        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("must not exceed 20 characters"));
    }

    [Fact]
    public async Task Validate_WhenDescriptionTooLong_ReturnsFailure()
    {
        var command = GetCommand(desc: new string('a', 101));
        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("Role description must not exceed 100 characters"));
    }

    [Fact]
    public async Task Validate_WhenPermissionsEmpty_ReturnsFailure()
    {
        var command = GetCommand(permissionIds: []);
        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("Permissions are required"));
    }

    [Fact]
    public async Task Validate_WhenSomePermissionsDoNotExist_ReturnsFailure()
    {
        _permissionRepositoryMock
            .Setup(r => r.GetExistingIdsAsync(It.IsAny<int[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([1, 2 ]);

        var command = GetCommand(permissionIds: [1, 2, 3]);
        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("One or more permissions do not exist"));
    }
}

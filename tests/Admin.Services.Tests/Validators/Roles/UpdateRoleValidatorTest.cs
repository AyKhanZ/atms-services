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
    private readonly UpdateRoleValidator _validator;

    public UpdateRoleValidatorTest()
    {
        _faker = new Faker();
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _validator = new UpdateRoleValidator(_roleRepositoryMock.Object);
        
        _roleRepositoryMock
            .Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<Role, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
    }
    
    private UpdateRoleCommand GetCommand(string? name = null, string? desc = null)
    {
        return new UpdateRoleCommand {
            Id = Guid.NewGuid(),
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
    public async Task Validate_WhenRoleNameAlreadyExists_ReturnsFailure()
    {
        _roleRepositoryMock
            .Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<Role, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var command = GetCommand();
 
        var result = await _validator.ValidateAsync(command);
 
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Role with this name already exists.");
    }
}

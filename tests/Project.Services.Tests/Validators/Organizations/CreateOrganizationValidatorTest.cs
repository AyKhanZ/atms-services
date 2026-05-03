using System.Linq.Expressions;
using ATMS.Project.Contracts.Commands.Organization;
using ATMS.Project.Data.Entities;
using ATMS.Project.Services.Validation.Organization;
using Moq;

namespace Project.Services.Tests.Validators.Organizations;

public class CreateOrganizationValidatorTest : BaseValidatorTest
{
    private readonly CreateOrganizationValidator _validator;

    public CreateOrganizationValidatorTest()
    {
        _validator = new CreateOrganizationValidator(OrganizationRepositoryMock.Object);
    }

    [Fact]
    public async Task Validate_WhenValid_PassesValidation()
    {
        OrganizationRepositoryMock
            .Setup(r => r.IsExistAsync(
                It.IsAny<Expression<Func<Organization, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new CreateOrganizationCommand
        {
            Title = Faker.Company.CompanyName(),
            Voen = Faker.Random.AlphaNumeric(10)
        };

        var result = await _validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WhenTitleEmpty_FailsValidation()
    {
        var command = new CreateOrganizationCommand
        {
            Title = "",
            Voen = Faker.Random.AlphaNumeric(10)
        };

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.Title));
    }

    [Fact]
    public async Task Validate_WhenTitleTooLong_FailsValidation()
    {
        var command = new CreateOrganizationCommand { 
            Title = new string('A', 201),
            Voen = Faker.Random.AlphaNumeric(10)
            
        };

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.Title));
    }

    [Fact]
    public async Task Validate_WhenTitleAlreadyExists_FailsValidation()
    {
        OrganizationRepositoryMock
            .Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<Organization, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new CreateOrganizationCommand
        {
            Title = Faker.Company.CompanyName(),
            Voen = Faker.Random.AlphaNumeric(10)
        };

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.Title));
    }

    [Fact]
    public async Task Validate_WhenVoenEmpty_FailsValidation()
    {
        var command = new CreateOrganizationCommand
        {
            Title = Faker.Company.CompanyName(),
            Voen = ""
        };

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.Voen));
    }

    [Fact]
    public async Task Validate_WhenVoenTooLong_FailsValidation()
    {
        OrganizationRepositoryMock
            .Setup(r => r.IsExistAsync(
                It.IsAny<Expression<Func<Organization, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new CreateOrganizationCommand
        {
            Title = Faker.Company.CompanyName(),
            Voen = new string('1', 21)
        };

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.Voen));
    }

    [Fact]
    public async Task Validate_WhenVoenAlreadyTaken_FailsValidation()
    {
        OrganizationRepositoryMock
            .SetupSequence(r => r.IsExistAsync(
                It.IsAny<Expression<Func<Organization, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false)  // Title is unique
            .ReturnsAsync(true);  // Voen is taken

        var command = new CreateOrganizationCommand {
            Title = Faker.Company.CompanyName(),
            Voen = Faker.Random.AlphaNumeric(10)
        };

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.Voen));
    }
}
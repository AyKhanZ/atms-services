using System.Linq.Expressions;
using ATMS.Project.Contracts.Commands.Organization;
using ATMS.Project.Data.Entities;
using ATMS.Project.Services.Validation.Organization;
using Moq;

namespace Project.Services.Tests.Validators.Organizations;

public class UpdateOrganizationValidatorTest : BaseValidatorTest
{
    private readonly UpdateOrganizationValidator _validator;

    public UpdateOrganizationValidatorTest()
    {
        _validator = new UpdateOrganizationValidator(OrganizationRepositoryMock.Object, CreateImagesConfiguration());
    }

    [Fact]
    public async Task Validate_WhenValid_PassesValidation()
    {
        OrganizationRepositoryMock
            .Setup(r => r.IsExistAsync(
                It.IsAny<Expression<Func<Organization, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new UpdateOrganizationCommand
        {
            Id = Guid.NewGuid(),
            Title = Faker.Company.CompanyName(),
            Voen = Faker.Random.AlphaNumeric(10)
        };

        var result = await _validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WhenIdEmpty_FailsValidation()
    {
        var command = new UpdateOrganizationCommand
        {
            Id = Guid.Empty,
            Title = Faker.Company.CompanyName(),
            Voen = Faker.Random.AlphaNumeric(10)
        };

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.Id));
    }

    [Fact]
    public async Task Validate_WhenTitleEmpty_FailsValidation()
    {
        var command = new UpdateOrganizationCommand
        {
            Id = Guid.NewGuid(),
            Title = "",
            Voen = Faker.Random.AlphaNumeric(10)
        };

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.Title));
    }

    [Fact]
    public async Task Validate_WhenVoenEmpty_FailsValidation()
    {
        var command = new UpdateOrganizationCommand
        {
            Id = Guid.NewGuid(),
            Title = Faker.Company.CompanyName(),
            Voen = ""
        };

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.Voen));
    }

    [Fact]
    public async Task Validate_WhenLogoIsEmpty_FailsValidation()
    {
        OrganizationRepositoryMock
            .Setup(r => r.IsExistAsync(
                It.IsAny<Expression<Func<Organization, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var logo = new Mock<Microsoft.AspNetCore.Http.IFormFile>();
        logo.SetupGet(x => x.Length).Returns(0);
        logo.SetupGet(x => x.ContentType).Returns("image/png");

        var command = new UpdateOrganizationCommand
        {
            Id = Guid.NewGuid(),
            Title = Faker.Company.CompanyName(),
            Voen = Faker.Random.AlphaNumeric(10),
            Logo = logo.Object
        };

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.Logo));
    }
}
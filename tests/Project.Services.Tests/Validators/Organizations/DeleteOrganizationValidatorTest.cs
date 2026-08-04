using ATMS.Project.Contracts.Commands.Organizations;
using ATMS.Project.Services.Validation.Organizations;

namespace Project.Services.Tests.Validators.Organizations;

public class DeleteOrganizationValidatorTest : BaseValidatorTest
{
    private readonly DeleteOrganizationValidator _validator = new();

    [Fact]
    public async Task Validate_WhenValid_PassesValidation()
    {
        var command = new DeleteOrganizationCommand { Id = Guid.NewGuid() };

        var result = await _validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WhenIdEmpty_FailsValidation()
    {
        var command = new DeleteOrganizationCommand { Id = Guid.Empty };

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.Id));
    }
}
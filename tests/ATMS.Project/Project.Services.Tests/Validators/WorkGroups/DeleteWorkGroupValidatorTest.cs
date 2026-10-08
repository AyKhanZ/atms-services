using ATMS.Project.Contracts.Commands.WorkGroups;
using ATMS.Project.Services.Validation.WorkGroups;

namespace Project.Services.Tests.Validators.WorkGroups;

public class DeleteWorkGroupValidatorTest
{
    private readonly DeleteWorkGroupValidator _validator = new();

    [Fact]
    public async Task Validate_WhenRouteIdsAreEmpty_FailsValidation()
    {
        var command = new DeleteWorkGroupCommand();

        var result = await _validator.ValidateAsync(command);

        Assert.Contains(result.Errors, x => x.PropertyName == nameof(command.ProjectId));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(command.WorkGroupId));
    }

    [Fact]
    public async Task Validate_WhenCommandIsValid_PassesValidation()
    {
        var command = new DeleteWorkGroupCommand
        {
            ProjectId = Guid.NewGuid(),
            WorkGroupId = Guid.NewGuid()
        };

        var result = await _validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }
}

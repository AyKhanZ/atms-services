using ATMS.Project.Contracts.Commands.WorkGroups;
using ATMS.Project.Services.Validation.WorkGroups;

namespace Project.Services.Tests.Validators.WorkGroups;

public class WorkGroupValidatorTest
{
    private readonly WorkGroupValidator _validator = new();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Create_WhenTitleIsBlank_FailsValidation(string title)
    {
        var command = new TestWorkGroupCommand { Title = title };

        var result = await _validator.ValidateAsync(command);

        Assert.Contains(result.Errors, x => x.PropertyName == nameof(command.Title));
    }

    [Fact]
    public async Task Create_WhenTrimmedTitleHasOneHundredCharacters_PassesValidation()
    {
        var command = new TestWorkGroupCommand { Title = $" {new string('A', 100)} " };

        var result = await _validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Create_WhenTrimmedTitleExceedsOneHundredCharacters_FailsValidation()
    {
        var command = new TestWorkGroupCommand { Title = new string('A', 101) };

        var result = await _validator.ValidateAsync(command);

        Assert.Contains(result.Errors, x => x.PropertyName == nameof(command.Title));
    }

    private sealed class TestWorkGroupCommand : WorkGroupCommand;
}

using ATMS.Project.Contracts.Commands.WorkGroups;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Validation.WorkGroups;
using Moq;

namespace Project.Services.Tests.Validators.WorkGroups;

public class CreateWorkGroupValidatorTest
{
    private readonly Mock<IWorkGroupRepository> _workGroupRepository = new();
    private readonly CreateWorkGroupValidator _validator;

    public CreateWorkGroupValidatorTest()
    {
        _validator = new CreateWorkGroupValidator(_workGroupRepository.Object);
    }

    [Fact]
    public async Task Validate_WhenProjectIdIsEmpty_FailsValidation()
    {
        var command = new CreateWorkGroupCommand { Title = "Group" };

        var result = await _validator.ValidateAsync(command);

        Assert.Contains(result.Errors, x => x.PropertyName == nameof(command.ProjectId));
    }

    [Fact]
    public async Task Validate_WhenParentIdIsEmpty_FailsValidation()
    {
        var command = new CreateWorkGroupCommand
        {
            ProjectId = Guid.NewGuid(),
            ParentWorkGroupId = Guid.Empty,
            Title = "Milestone"
        };

        var result = await _validator.ValidateAsync(command);

        Assert.Contains(result.Errors, x => x.PropertyName == nameof(command.ParentWorkGroupId));
    }

    [Fact]
    public async Task Validate_WhenCommandIsValid_PassesValidation()
    {
        var command = new CreateWorkGroupCommand
        {
            ProjectId = Guid.NewGuid(),
            ParentWorkGroupId = Guid.NewGuid(),
            Title = "Milestone"
        };

        var result = await _validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(null, "A group with this name already exists. Choose a different name.")]
    [InlineData("D986750A-8C4C-4F09-B82B-98B1D4F94B54", "This group already has a milestone with this name. Choose a different name.")]
    public async Task Validate_WhenSiblingTitleExists_ReturnsSpecificMessage(
        string? parentWorkGroupId,
        string expectedMessage)
    {
        var command = new CreateWorkGroupCommand
        {
            ProjectId = Guid.NewGuid(),
            ParentWorkGroupId = parentWorkGroupId is null ? null : Guid.Parse(parentWorkGroupId),
            Title = " Delivery "
        };
        _workGroupRepository
            .Setup(x => x.IsSiblingTitleExistAsync(
                command.ProjectId,
                command.ParentWorkGroupId,
                "delivery",
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _validator.ValidateAsync(command);

        var error = Assert.Single(result.Errors, x => x.PropertyName == nameof(command.Title));
        Assert.Equal(expectedMessage, error.ErrorMessage);
    }
}

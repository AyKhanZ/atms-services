using ATMS.Project.Contracts.Commands.WorkGroups;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Validation.WorkGroups;
using Moq;

namespace Project.Services.Tests.Validators.WorkGroups;

public class UpdateWorkGroupValidatorTest
{
    private readonly Mock<IWorkGroupRepository> _workGroupRepository = new();
    private readonly UpdateWorkGroupValidator _validator;

    public UpdateWorkGroupValidatorTest()
    {
        _validator = new UpdateWorkGroupValidator(_workGroupRepository.Object);
    }

    [Fact]
    public async Task Validate_WhenRouteIdsAreEmpty_FailsValidation()
    {
        var command = new UpdateWorkGroupCommand { Title = "Group" };

        var result = await _validator.ValidateAsync(command);

        Assert.Contains(result.Errors, x => x.PropertyName == nameof(command.ProjectId));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(command.WorkGroupId));
    }

    [Fact]
    public async Task Validate_WhenCommandIsValid_PassesValidation()
    {
        var command = new UpdateWorkGroupCommand
        {
            ProjectId = Guid.NewGuid(),
            WorkGroupId = Guid.NewGuid(),
            Title = "Group"
        };

        _workGroupRepository
            .Setup(x => x.FindAsync(command.ProjectId, command.WorkGroupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WorkGroup { Id = command.WorkGroupId, WorkProjectId = command.ProjectId });

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
        var command = new UpdateWorkGroupCommand
        {
            ProjectId = Guid.NewGuid(),
            WorkGroupId = Guid.NewGuid(),
            Title = " Delivery "
        };
        var parentId = parentWorkGroupId is null ? (Guid?)null : Guid.Parse(parentWorkGroupId);
        _workGroupRepository
            .Setup(x => x.FindAsync(command.ProjectId, command.WorkGroupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WorkGroup
            {
                Id = command.WorkGroupId,
                WorkProjectId = command.ProjectId,
                ParentWorkGroupId = parentId
            });
        _workGroupRepository
            .Setup(x => x.IsSiblingTitleExistAsync(
                command.ProjectId,
                parentId,
                "delivery",
                command.WorkGroupId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _validator.ValidateAsync(command);

        var error = Assert.Single(result.Errors, x => x.PropertyName == nameof(command.Title));
        Assert.Equal(expectedMessage, error.ErrorMessage);
    }
}

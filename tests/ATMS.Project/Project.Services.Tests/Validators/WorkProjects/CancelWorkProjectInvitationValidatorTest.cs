using System.Linq.Expressions;
using ATMS.Project.Contracts.Commands.WorkProjects;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Validation.WorkProjects;
using Moq;

namespace Project.Services.Tests.Validators.WorkProjects;

public class CancelWorkProjectInvitationValidatorTest : BaseValidatorTest
{
    private readonly Mock<IWorkProjectInvitationRepository> _invitationRepositoryMock = new();
    private readonly CancelWorkProjectInvitationValidator _validator;

    public CancelWorkProjectInvitationValidatorTest()
    {
        WorkProjectsRepositoryMock
            .Setup(x => x.IsExistAsync(It.IsAny<Expression<Func<WorkProject, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _validator = new CancelWorkProjectInvitationValidator(
            WorkProjectsRepositoryMock.Object,
            _invitationRepositoryMock.Object);
    }

    [Fact]
    public async Task Validate_WhenInvitationIsPending_PassesValidation()
    {
        var command = CreateCommand();
        _invitationRepositoryMock
            .Setup(x => x.IsPendingAsync(command.ProjectId, command.InvitationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    // Accepted a moment ago or already cancelled: nothing is left to cancel.
    [Fact]
    public async Task Validate_WhenInvitationIsNotPending_FailsOnInvitation()
    {
        var result = await _validator.ValidateAsync(CreateCommand());

        Assert.Contains(result.Errors, x => x.PropertyName == nameof(CancelWorkProjectInvitationCommand.InvitationId));
    }

    [Fact]
    public async Task Validate_WhenProjectIsMissing_FailsOnProjectAndSkipsInvitationLookup()
    {
        WorkProjectsRepositoryMock
            .Setup(x => x.IsExistAsync(It.IsAny<Expression<Func<WorkProject, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _validator.ValidateAsync(CreateCommand());

        Assert.Contains(result.Errors, x => x.PropertyName == nameof(CancelWorkProjectInvitationCommand.ProjectId));
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task Validate_WhenIdIsEmpty_FailsOnThatId(bool emptyProject, bool emptyInvitation)
    {
        var command = new CancelWorkProjectInvitationCommand
        {
            ProjectId = emptyProject ? Guid.Empty : Guid.NewGuid(),
            InvitationId = emptyInvitation ? Guid.Empty : Guid.NewGuid()
        };

        var result = await _validator.ValidateAsync(command);

        Assert.Contains(result.Errors, x => x.PropertyName == (emptyProject
            ? nameof(command.ProjectId)
            : nameof(command.InvitationId)));
    }

    private static CancelWorkProjectInvitationCommand CreateCommand()
    {
        return new CancelWorkProjectInvitationCommand
        {
            ProjectId = Guid.NewGuid(),
            InvitationId = Guid.NewGuid()
        };
    }
}

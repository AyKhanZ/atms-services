using ATMS.Project.Contracts.Commands.WorkTickets;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Validation.WorkTickets;
using Moq;

namespace Project.Services.Tests.Validators.WorkTickets;

public class CreateWorkTicketValidatorTest
{
    private readonly Mock<IWorkTicketRepository> _workTicketRepository = new();
    private readonly Mock<IDictionariesRepository> _dictionariesRepository = new();
    private readonly CreateWorkTicketValidator _validator;

    public CreateWorkTicketValidatorTest()
    {
        _workTicketRepository
            .Setup(repository => repository.IsMilestoneExistAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _workTicketRepository
            .Setup(repository => repository.IsProjectParticipantExistAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _dictionariesRepository
            .Setup(repository => repository.IsWorkTicketTypeExistAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _dictionariesRepository
            .Setup(repository => repository.IsWorkItemPriorityExistAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _validator = new CreateWorkTicketValidator(
            _workTicketRepository.Object,
            _dictionariesRepository.Object);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validate_WhenTitleIsBlank_FailsValidation(string title)
    {
        var command = ValidCommand();
        command.Title = title;

        var result = await _validator.ValidateAsync(command);

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(command.Title));
    }

    [Fact]
    public async Task Validate_WhenProjectIdIsEmpty_FailsValidation()
    {
        var command = ValidCommand();
        command.ProjectId = Guid.Empty;

        var result = await _validator.ValidateAsync(command);

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(command.ProjectId));
    }

    [Fact]
    public async Task Validate_WhenMilestoneDoesNotBelongToProject_FailsValidation()
    {
        var command = ValidCommand();
        _workTicketRepository
            .Setup(repository => repository.IsMilestoneExistAsync(
                command.ProjectId,
                command.MilestoneId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _validator.ValidateAsync(command);

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(command.MilestoneId));
    }

    [Fact]
    public async Task Validate_WhenAssigneeIsNotProjectParticipant_FailsValidation()
    {
        var command = ValidCommand();
        command.AssigneeId = Guid.NewGuid();
        _workTicketRepository
            .Setup(repository => repository.IsProjectParticipantExistAsync(
                command.ProjectId,
                command.AssigneeId.Value,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _validator.ValidateAsync(command);

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(command.AssigneeId));
    }

    private static CreateWorkTicketCommand ValidCommand()
    {
        return new CreateWorkTicketCommand
        {
            ProjectId = Guid.NewGuid(),
            Title = "Ticket",
            MilestoneId = Guid.NewGuid(),
            WorkTicketTypeId = 1,
            PriorityId = 1
        };
    }
}

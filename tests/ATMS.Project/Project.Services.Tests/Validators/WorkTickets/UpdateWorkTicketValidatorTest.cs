using ATMS.Project.Contracts.Commands.WorkTickets;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Validation.WorkTickets;
using Moq;

namespace Project.Services.Tests.Validators.WorkTickets;

public class UpdateWorkTicketValidatorTest
{
    private readonly UpdateWorkTicketValidator _validator;

    public UpdateWorkTicketValidatorTest()
    {
        var workTicketRepository = new Mock<IWorkTicketRepository>();
        var dictionariesRepository = new Mock<IDictionariesRepository>();
        workTicketRepository
            .Setup(repository => repository.IsMilestoneExistAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        dictionariesRepository
            .Setup(repository => repository.IsWorkTicketTypeExistAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        dictionariesRepository
            .Setup(repository => repository.IsWorkItemPriorityExistAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        dictionariesRepository
            .Setup(repository => repository.IsWorkTicketStatusExistAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _validator = new UpdateWorkTicketValidator(
            workTicketRepository.Object,
            dictionariesRepository.Object);
    }

    [Theory]
    [InlineData(true, false, nameof(UpdateWorkTicketCommand.ProjectId))]
    [InlineData(false, true, nameof(UpdateWorkTicketCommand.WorkTicketId))]
    public async Task Validate_WhenRouteIdIsEmpty_FailsValidation(
        bool emptyProjectId,
        bool emptyTicketId,
        string expectedProperty)
    {
        var command = new UpdateWorkTicketCommand
        {
            ProjectId = emptyProjectId ? Guid.Empty : Guid.NewGuid(),
            WorkTicketId = emptyTicketId ? Guid.Empty : Guid.NewGuid(),
            Title = "Ticket",
            MilestoneId = Guid.NewGuid(),
            WorkTicketTypeId = 1,
            PriorityId = 1,
            WorkTicketStatusId = 1
        };

        var result = await _validator.ValidateAsync(command);

        Assert.Contains(result.Errors, error => error.PropertyName == expectedProperty);
    }

    [Fact]
    public async Task Validate_WhenStatusDoesNotExist_FailsValidation()
    {
        var workTicketRepository = new Mock<IWorkTicketRepository>();
        var dictionariesRepository = new Mock<IDictionariesRepository>();
        workTicketRepository
            .Setup(repository => repository.IsMilestoneExistAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        dictionariesRepository
            .Setup(repository => repository.IsWorkTicketTypeExistAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        dictionariesRepository
            .Setup(repository => repository.IsWorkItemPriorityExistAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        dictionariesRepository
            .Setup(repository => repository.IsWorkTicketStatusExistAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var validator = new UpdateWorkTicketValidator(
            workTicketRepository.Object,
            dictionariesRepository.Object);

        var result = await validator.ValidateAsync(new UpdateWorkTicketCommand
        {
            ProjectId = Guid.NewGuid(),
            WorkTicketId = Guid.NewGuid(),
            Title = "Ticket",
            MilestoneId = Guid.NewGuid(),
            WorkTicketTypeId = 1,
            PriorityId = 1,
            WorkTicketStatusId = 999
        });

        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(UpdateWorkTicketCommand.WorkTicketStatusId));
    }
}

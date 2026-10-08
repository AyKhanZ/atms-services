using ATMS.Project.Contracts.Commands.WorkTickets;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Validation.WorkTickets;
using Moq;

namespace Project.Services.Tests.Validators.WorkTickets;

public class DeleteWorkTicketValidatorTest
{
    private readonly Mock<IWorkTicketRepository> _tickets = new();

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public async Task Validate_RejectsTicketWhenItHasTasks(bool hasTasks, bool expectedValid)
    {
        var command = new DeleteWorkTicketCommand
        {
            ProjectId = Guid.NewGuid(),
            WorkTicketId = Guid.NewGuid()
        };
        _tickets.Setup(repository => repository.HasTasksAsync(
                command.ProjectId,
                command.WorkTicketId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(hasTasks);

        var result = await new DeleteWorkTicketValidator(_tickets.Object).ValidateAsync(command);

        Assert.Equal(expectedValid, result.IsValid);
        if (hasTasks)
        {
            Assert.Contains(result.Errors, error =>
                error.PropertyName == nameof(command.WorkTicketId));
        }
    }

    [Fact]
    public async Task Validate_WhenIdentifiersAreEmpty_ReturnsRequiredErrorsWithoutQueryingTasks()
    {
        var command = new DeleteWorkTicketCommand();

        var result = await new DeleteWorkTicketValidator(_tickets.Object).ValidateAsync(command);

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(command.ProjectId));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(command.WorkTicketId));
        _tickets.Verify(repository => repository.HasTasksAsync(
            It.IsAny<Guid>(),
            It.IsAny<Guid>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }
}

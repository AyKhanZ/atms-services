using ATMS.Data.Enums;
using ATMS.Project.Contracts.Commands.WorkTickets;
using ATMS.Project.Data.Entities;
using ATMS.Project.Services.Handlers.WorkTickets;
using Moq;

namespace Project.Services.Tests.Handlers.WorkTickets;

// Closing a ticket with open work: "Mark all as done" closes the tasks and subtasks with it.
public class CloseWorkTicketWithTasksTest : BaseHandlerTest
{
    private readonly Guid _projectId = Guid.NewGuid();
    private readonly WorkTicket _ticket = new() { Id = Guid.NewGuid() };

    private WorkTask[] Arrange()
    {
        _ticket.WorkProjectId = _projectId;
        var tasks = new[]
        {
            new WorkTask { Id = Guid.NewGuid(), StatusId = (int)WorkTaskStatusEnum.New },
            new WorkTask { Id = Guid.NewGuid(), StatusId = (int)WorkTaskStatusEnum.InProgress }
        };
        WorkTicketRepositoryMock
            .Setup(repository => repository.FindAsync(_projectId, _ticket.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_ticket);
        WorkTaskRepositoryMock
            .Setup(repository => repository.FindByTicketAsync(_projectId, _ticket.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tasks);
        WorkTaskRepositoryMock
            .Setup(repository => repository.GetIdsByTicketsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        return tasks;
    }

    private Task Save(int statusId, bool completeTasks) =>
        new UpdateWorkTicketHandler(
                MapperMock.Object,
                WorkTicketRepositoryMock.Object,
                WorkTaskRepositoryMock.Object,
                CacheServiceMock.Object)
            .Handle(new UpdateWorkTicketCommand
            {
                ProjectId = _projectId,
                WorkTicketId = _ticket.Id,
                Title = "Ticket",
                MilestoneId = Guid.NewGuid(),
                WorkTicketStatusId = statusId,
                CompleteTasks = completeTasks
            }, CancellationToken.None);

    [Fact]
    public async Task Handle_WhenClosedWithCompleteTasks_ClosesTheOpenWork()
    {
        var tasks = Arrange();

        await Save((int)WorkTicketStatusEnum.Closed, completeTasks: true);

        Assert.All(tasks, task =>
        {
            Assert.Equal((int)WorkTaskStatusEnum.Done, task.StatusId);
            Assert.NotNull(task.DoneAt);
        });
    }

    // Only this ticket: the work stays as it is.
    [Fact]
    public async Task Handle_WhenClosedWithoutCompleteTasks_LeavesTheWorkAlone()
    {
        var tasks = Arrange();

        await Save((int)WorkTicketStatusEnum.Closed, completeTasks: false);

        Assert.DoesNotContain(tasks, task => task.StatusId == (int)WorkTaskStatusEnum.Done);
    }
}

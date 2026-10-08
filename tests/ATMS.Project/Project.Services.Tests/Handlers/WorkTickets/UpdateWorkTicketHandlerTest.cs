using ATMS.Project.Contracts.Commands.WorkTickets;
using ATMS.Project.Data.Entities;
using ATMS.Project.Services.Handlers.WorkTickets;
using ATMS.Caching.Constants;
using Moq;

namespace Project.Services.Tests.Handlers.WorkTickets;

public class UpdateWorkTicketHandlerTest : BaseHandlerTest
{
    [Fact]
    public async Task Handle_WhenTicketExists_UpdatesFieldsAndInvalidatesCacheEntry()
    {
        var projectId = Guid.NewGuid();
        var ticket = new WorkTicket { Id = Guid.NewGuid(), WorkProjectId = projectId };
        var taskIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var command = new UpdateWorkTicketCommand
        {
            ProjectId = projectId,
            WorkTicketId = ticket.Id,
            Title = "Updated title",
            Description = "Updated description",
            MilestoneId = Guid.NewGuid(),
            WorkTicketTypeId = 2,
            PriorityId = 3,
            WorkTicketStatusId = 2,
            Deadline = DateTime.UtcNow.AddDays(5),
            AssigneeId = Guid.NewGuid()
        };
        WorkTicketRepositoryMock
            .Setup(repository => repository.FindAsync(
                projectId,
                ticket.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);
        WorkTaskRepositoryMock
            .Setup(repository => repository.GetIdsByTicketsAsync(
                It.Is<IReadOnlyCollection<Guid>>(ids => ids.SequenceEqual(new[] { ticket.Id })),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskIds);
        MapperMock
            .Setup(mapper => mapper.Map(command, ticket))
            .Callback<UpdateWorkTicketCommand, WorkTicket>((source, destination) =>
            {
                destination.Title = source.Title;
                destination.Description = source.Description;
                destination.WorkGroupId = source.MilestoneId;
                destination.WorkTicketTypeId = source.WorkTicketTypeId;
                destination.PriorityId = source.PriorityId;
                destination.WorkTicketStatusId = source.WorkTicketStatusId;
                destination.Deadline = source.Deadline;
                destination.AssigneeId = source.AssigneeId;
            });
        var handler = new UpdateWorkTicketHandler(
            MapperMock.Object,
            WorkTicketRepositoryMock.Object,
            WorkTaskRepositoryMock.Object,
            CacheServiceMock.Object);

        await handler.Handle(command, CancellationToken.None);

        Assert.Equal("Updated title", ticket.Title);
        Assert.Equal("Updated description", ticket.Description);
        Assert.Equal(2, ticket.WorkTicketStatusId);
        MapperMock.Verify(mapper => mapper.Map(command, ticket), Times.Once);
        WorkTicketRepositoryMock.Verify(
            repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
        VerifyAllLocalizedCacheEntriesRemoved(
            language => CacheKeys.Project.TicketById(ticket.Id, language));
        foreach (var taskId in taskIds)
        {
            VerifyAllLocalizedCacheEntriesRemoved(
                language => CacheKeys.Project.TaskById(taskId, language));
        }
    }

    [Fact]
    public async Task Handle_WhenTicketDoesNotExist_ThrowsAndDoesNotInvalidateCache()
    {
        WorkTicketRepositoryMock
            .Setup(repository => repository.FindAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkTicket?)null);
        var handler = new UpdateWorkTicketHandler(
            MapperMock.Object,
            WorkTicketRepositoryMock.Object,
            WorkTaskRepositoryMock.Object,
            CacheServiceMock.Object);

        await Assert.ThrowsAsync<ATMS.Application.Exceptions.Entity.EntityException>(() => handler.Handle(
            new UpdateWorkTicketCommand
            {
                ProjectId = Guid.NewGuid(),
                WorkTicketId = Guid.NewGuid(),
                Title = "Ticket",
                MilestoneId = Guid.NewGuid(),
                WorkTicketStatusId = 1
            },
            CancellationToken.None));

        CacheServiceMock.VerifyNoOtherCalls();
    }
}

using ATMS.Application.Exceptions.Entity;
using ATMS.Caching.Constants;
using ATMS.Project.Contracts.Commands.WorkTickets;
using ATMS.Project.Data.Entities;
using ATMS.Project.Services.Handlers.WorkTickets;
using Moq;

namespace Project.Services.Tests.Handlers.WorkTickets;

public class DeleteWorkTicketHandlerTest : BaseHandlerTest
{
    [Fact]
    public async Task Handle_WhenTicketExists_SoftDeletesTicketAndInvalidatesCache()
    {
        var userId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var ticket = new WorkTicket { Id = Guid.NewGuid(), WorkProjectId = projectId };
        var command = new DeleteWorkTicketCommand
        {
            ProjectId = projectId,
            WorkTicketId = ticket.Id
        };
        CurrentUserMock.SetupGet(user => user.Id).Returns(userId);
        WorkTicketRepositoryMock
            .Setup(repository => repository.FindAsync(projectId, ticket.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);
        var handler = new DeleteWorkTicketHandler(
            CurrentUserMock.Object,
            WorkTicketRepositoryMock.Object,
            CacheServiceMock.Object);

        await handler.Handle(command, CancellationToken.None);

        Assert.True(ticket.IsDeleted);
        Assert.NotNull(ticket.DeletedAt);
        Assert.Equal(userId, ticket.DeletedById);
        WorkTicketRepositoryMock.Verify(
            repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
        CacheServiceMock.Verify(
            cache => cache.RemoveAsync(
                CacheKeys.Project.TicketById(ticket.Id),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenTicketDoesNotExist_ThrowsAndDoesNotMutateDataOrCache()
    {
        WorkTicketRepositoryMock
            .Setup(repository => repository.FindAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkTicket?)null);
        var handler = new DeleteWorkTicketHandler(
            CurrentUserMock.Object,
            WorkTicketRepositoryMock.Object,
            CacheServiceMock.Object);

        await Assert.ThrowsAsync<EntityException>(() => handler.Handle(
            new DeleteWorkTicketCommand
            {
                ProjectId = Guid.NewGuid(),
                WorkTicketId = Guid.NewGuid()
            },
            CancellationToken.None));

        WorkTicketRepositoryMock.Verify(
            repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
        CacheServiceMock.VerifyNoOtherCalls();
    }
}

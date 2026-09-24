using ATMS.Data.Criteria;
using ATMS.Project.Contracts.Requests.Attachments;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Models.Attachments;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Handlers.Attachments;
using Moq;

namespace Project.Services.Tests.Handlers.Attachments;

public class GetAttachmentsTreeHandlerTest : BaseHandlerTest
{
    private readonly Mock<IAttachmentRepository> _attachmentRepositoryMock = new();

    private static readonly DateTime Day = new(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc);

    private static AttachmentTicketCount Ticket(
        Guid groupId, int groupDay, Guid milestoneId, int milestoneDay, string code, int ticketDay, int files) =>
        new(
            groupId,
            $"Group {groupDay}",
            Day.AddDays(groupDay),
            milestoneId,
            $"Milestone {milestoneDay}",
            Day.AddDays(milestoneDay),
            Guid.NewGuid(),
            code,
            $"Ticket {code}",
            Day.AddDays(ticketDay),
            files);

    [Fact]
    public async Task Handle_BuildsGroupsMilestonesAndTicketsInPlanOrderWithSummedCounts()
    {
        var backend = Guid.NewGuid();
        var frontend = Guid.NewGuid();
        var sprint1 = Guid.NewGuid();
        var sprint2 = Guid.NewGuid();
        var design = Guid.NewGuid();
        _attachmentRepositoryMock
            .Setup(repository => repository.GetTicketCountsAsync(It.IsAny<ACriteria<WorkTask>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                Ticket(frontend, 2, design, 3, "40", 4, 1),
                Ticket(backend, 1, sprint2, 5, "30", 6, 4),
                Ticket(backend, 1, sprint1, 2, "29", 8, 3),
                Ticket(backend, 1, sprint1, 2, "28", 7, 2)
            ]);

        var tree = await new GetAttachmentsTreeHandler(_attachmentRepositoryMock.Object)
            .Handle(new GetAttachmentsTreeRequest { ProjectId = Guid.NewGuid() }, CancellationToken.None);

        Assert.Equal(10, tree.FileCount);
        Assert.Equal([backend, frontend], tree.Groups.Select(group => group.Id));
        var first = tree.Groups.First();
        Assert.Equal(9, first.FileCount);
        Assert.Equal([sprint1, sprint2], first.Milestones.Select(milestone => milestone.Id));
        Assert.Equal(5, first.Milestones.First().FileCount);
        Assert.Equal(["28", "29"], first.Milestones.First().Tickets.Select(ticket => ticket.WorkTicket.Code));
        Assert.Equal("Ticket 28", first.Milestones.First().Tickets.First().WorkTicket.Name);
    }

    [Fact]
    public async Task Handle_WhenProjectHasNoFiles_ReturnsAnEmptyTree()
    {
        _attachmentRepositoryMock
            .Setup(repository => repository.GetTicketCountsAsync(It.IsAny<ACriteria<WorkTask>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var tree = await new GetAttachmentsTreeHandler(_attachmentRepositoryMock.Object)
            .Handle(new GetAttachmentsTreeRequest { ProjectId = Guid.NewGuid() }, CancellationToken.None);

        Assert.Equal(0, tree.FileCount);
        Assert.Empty(tree.Groups);
    }
}

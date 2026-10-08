using ATMS.Data.Enums;
using ATMS.Project.Contracts.Commands.WorkTickets;
using ATMS.Project.Data.Entities;
using ATMS.Project.Services.Handlers.WorkTickets;
using Moq;

namespace Project.Services.Tests.Handlers.WorkTickets;

public class CreateWorkTicketHandlerTest : BaseHandlerTest
{
    [Fact]
    public async Task Handle_WhenCommandIsValid_CreatesMappedNewTicket()
    {
        var projectId = Guid.NewGuid();
        var milestoneId = Guid.NewGuid();
        var assigneeId = Guid.NewGuid();
        var command = new CreateWorkTicketCommand
        {
            ProjectId = projectId,
            Title = "Prepare release",
            Description = "Release checklist",
            MilestoneId = milestoneId,
            WorkTicketTypeId = (int)WorkTicketTypeEnum.Project,
            PriorityId = (int)WorkItemPriorityEnum.High,
            AssigneeId = assigneeId
        };
        var mappedTicket = new WorkTicket
        {
            Title = command.Title,
            Description = command.Description,
            WorkProjectId = command.ProjectId,
            WorkGroupId = command.MilestoneId,
            WorkTicketTypeId = command.WorkTicketTypeId,
            PriorityId = command.PriorityId,
            AssigneeId = command.AssigneeId
        };
        WorkTicket? created = null;
        MapperMock
            .Setup(mapper => mapper.Map<WorkTicket>(command))
            .Returns(mappedTicket);
        EntityCodeGeneratorMock
            .Setup(generator => generator.GetNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("1042");
        WorkTicketRepositoryMock
            .Setup(repository => repository.CreateAsync(
                It.IsAny<WorkTicket>(),
                It.IsAny<CancellationToken>()))
            .Callback<WorkTicket, CancellationToken>((ticket, _) => created = ticket);
        var handler = new CreateWorkTicketHandler(
            MapperMock.Object,
            WorkTicketRepositoryMock.Object,
            EntityCodeGeneratorMock.Object);

        var id = await handler.Handle(command, CancellationToken.None);

        Assert.NotNull(created);
        Assert.Equal(id, created.Id);
        Assert.Equal("1042", created.Code);
        Assert.Equal("Prepare release", created.Title);
        Assert.Equal("Release checklist", created.Description);
        Assert.Equal(projectId, created.WorkProjectId);
        Assert.Equal(milestoneId, created.WorkGroupId);
        Assert.Equal((int)WorkTicketStatusEnum.New, created.WorkTicketStatusId);
        Assert.Equal((int)WorkTaskStatusEnum.New, created.StatusId);
        Assert.Equal(assigneeId, created.AssigneeId);
        MapperMock.Verify(mapper => mapper.Map<WorkTicket>(command), Times.Once);
    }
}

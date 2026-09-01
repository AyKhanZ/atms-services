using ATMS.Project.API.Controllers.v1;
using ATMS.Project.Contracts.Commands.WorkTickets;
using ATMS.Project.Contracts.Models.WorkTickets;
using ATMS.Project.Contracts.Requests.WorkTickets;
using ATMS.Data.Criteria;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Project.API.Tests;

public class WorkTicketControllerTest : BaseControllerTest
{
    private readonly WorkTicketController _controller;

    public WorkTicketControllerTest()
    {
        _controller = new WorkTicketController(MediatorMock.Object);
    }

    [Fact]
    public async Task Create_OverridesRouteProjectIdAndReturns201()
    {
        var projectId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var command = new CreateWorkTicketCommand
        {
            ProjectId = Guid.NewGuid(),
            Title = "Ticket",
            MilestoneId = Guid.NewGuid(),
            WorkTicketTypeId = 1,
            PriorityId = 1
        };
        MediatorMock
            .Setup(mediator => mediator.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticketId);

        var result = await _controller.Create(projectId, command, CancellationToken.None);

        Assert.Equal(projectId, command.ProjectId);
        var response = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(StatusCodes.Status201Created, response.StatusCode);
        Assert.Equal(ticketId, response.Value);
    }

    [Fact]
    public async Task Get_UsesRouteIdsAndReturnsTicket()
    {
        var projectId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var expected = CreateModel(ticketId);
        MediatorMock
            .Setup(mediator => mediator.Send(
                It.Is<GetWorkTicketRequest>(request =>
                    request.ProjectId == projectId && request.WorkTicketId == ticketId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.Get(projectId, ticketId, CancellationToken.None);

        var response = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, response.Value);
    }

    [Fact]
    public async Task GetMany_OverridesRouteProjectIdAndPreservesCursorFilter()
    {
        var projectId = Guid.NewGuid();
        var milestoneId = Guid.NewGuid();
        var request = new GetWorkTicketsRequest
        {
            ProjectId = Guid.NewGuid(),
            MilestoneId = milestoneId,
            Cursor = "cursor",
            PageSize = 10
        };
        var expected = new KeysetPagedResult<WorkTicketModel> { PageSize = 10 };
        MediatorMock
            .Setup(mediator => mediator.Send(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetMany(projectId, request, CancellationToken.None);

        Assert.Equal(projectId, request.ProjectId);
        Assert.Equal(milestoneId, request.MilestoneId);
        Assert.Equal("cursor", request.Cursor);
        var response = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, response.Value);
    }

    [Fact]
    public async Task Update_OverridesRouteIdsAndPreservesSelectedStatus()
    {
        var projectId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var command = new UpdateWorkTicketCommand
        {
            ProjectId = Guid.NewGuid(),
            WorkTicketId = Guid.NewGuid(),
            Title = "Updated ticket",
            MilestoneId = Guid.NewGuid(),
            WorkTicketTypeId = 2,
            PriorityId = 3,
            WorkTicketStatusId = 4
        };
        MediatorMock
            .Setup(mediator => mediator.Send(command, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.Update(projectId, ticketId, command, CancellationToken.None);

        Assert.Equal(projectId, command.ProjectId);
        Assert.Equal(ticketId, command.WorkTicketId);
        Assert.Equal(4, command.WorkTicketStatusId);
        Assert.IsType<NoContentResult>(result);
    }

    private static WorkTicketModel CreateModel(Guid id) => new()
    {
        Id = id,
        Code = "T-1",
        Title = "Ticket",
        MilestoneTitle = "Milestone",
        GroupTitle = "Group",
        WorkTicketType = new(),
        WorkTicketStatus = new(),
        Priority = new()
    };
}

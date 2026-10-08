using ATMS.Data.Criteria;
using ATMS.Project.API.Controllers.v1;
using ATMS.Project.Contracts.Models.History;
using ATMS.Project.Contracts.Requests.History;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Project.API.Tests;

public class HistoryControllerTest : BaseControllerTest
{
    private readonly HistoryController _controller;

    public HistoryControllerTest()
    {
        _controller = new HistoryController(MediatorMock.Object);
    }

    [Fact]
    public async Task GetMany_TakesTheProjectFromTheRouteAndReturnsThePage()
    {
        var projectId = Guid.NewGuid();
        var request = new GetHistoryRequest { ProjectId = Guid.NewGuid() };
        var page = new KeysetPagedResult<HistoryEntryModel>();
        MediatorMock.Setup(mediator => mediator.Send(request, It.IsAny<CancellationToken>())).ReturnsAsync(page);

        var result = await _controller.GetMany(projectId, request, CancellationToken.None);

        Assert.Equal(projectId, request.ProjectId);
        Assert.Same(page, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task GetStates_TakesTheProjectFromTheRouteAndReturnsTheStates()
    {
        var projectId = Guid.NewGuid();
        var request = new GetHistoryStatesRequest { ProjectId = Guid.NewGuid() };
        IReadOnlyCollection<HistoryStateModel> states = [new HistoryStateModel()];
        MediatorMock.Setup(mediator => mediator.Send(request, It.IsAny<CancellationToken>())).ReturnsAsync(states);

        var result = await _controller.GetStates(projectId, request, CancellationToken.None);

        Assert.Equal(projectId, request.ProjectId);
        Assert.Same(states, Assert.IsType<OkObjectResult>(result.Result).Value);
    }
}

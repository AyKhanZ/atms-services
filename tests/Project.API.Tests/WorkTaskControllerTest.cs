using ATMS.Data.Criteria;
using ATMS.Project.API.Controllers.v1;
using ATMS.Project.Contracts.Commands.WorkTasks;
using ATMS.Project.Contracts.Models.WorkTasks;
using ATMS.Project.Contracts.Requests.WorkTasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Project.API.Tests;

public class WorkTaskControllerTest : BaseControllerTest
{
    private readonly WorkTaskController _controller;

    public WorkTaskControllerTest()
    {
        _controller = new WorkTaskController(MediatorMock.Object);
    }

    [Fact]
    public async Task Create_OverridesRouteProjectIdAndReturns201()
    {
        var projectId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var command = new CreateWorkTaskCommand { ProjectId = Guid.NewGuid(), WorkTicketId = Guid.NewGuid(), Title = "Task", PriorityId = 1 };
        MediatorMock.Setup(mediator => mediator.Send(command, It.IsAny<CancellationToken>())).ReturnsAsync(taskId);

        var result = await _controller.Create(projectId, command, CancellationToken.None);

        Assert.Equal(projectId, command.ProjectId);
        var response = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(StatusCodes.Status201Created, response.StatusCode);
        Assert.Equal(taskId, response.Value);
    }

    [Fact]
    public async Task Update_OverridesBothRouteIds()
    {
        var projectId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var command = new UpdateWorkTaskCommand { ProjectId = Guid.NewGuid(), WorkTaskId = Guid.NewGuid(), Title = "Task", PriorityId = 1, StatusId = 2 };
        MediatorMock.Setup(mediator => mediator.Send(command, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _controller.Update(projectId, taskId, command, CancellationToken.None);

        Assert.Equal(projectId, command.ProjectId);
        Assert.Equal(taskId, command.WorkTaskId);
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task GetMany_PreservesHierarchyFiltersAndOverridesProjectId()
    {
        var projectId = Guid.NewGuid();
        var request = new GetWorkTasksRequest { ProjectId = Guid.NewGuid(), ParentWorkTaskId = Guid.NewGuid(), PageSize = 10 };
        var expected = new KeysetPagedResult<WorkTaskModel>();
        MediatorMock.Setup(mediator => mediator.Send(request, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var result = await _controller.GetMany(projectId, request, CancellationToken.None);

        Assert.Equal(projectId, request.ProjectId);
        Assert.NotNull(request.ParentWorkTaskId);
        Assert.Same(expected, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task Delete_SendsProjectScopedCommandAndReturns204()
    {
        var projectId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        DeleteWorkTaskCommand? sentCommand = null;
        MediatorMock.Setup(mediator => mediator.Send(It.IsAny<DeleteWorkTaskCommand>(), It.IsAny<CancellationToken>()))
            .Callback<DeleteWorkTaskCommand, CancellationToken>((command, _) => sentCommand = command)
            .Returns(Task.CompletedTask);

        var result = await _controller.Delete(projectId, taskId, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        Assert.NotNull(sentCommand);
        Assert.Equal(projectId, sentCommand.ProjectId);
        Assert.Equal(taskId, sentCommand.WorkTaskId);
    }
}

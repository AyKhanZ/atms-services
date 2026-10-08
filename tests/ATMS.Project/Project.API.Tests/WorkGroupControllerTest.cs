using ATMS.Project.API.Controllers.v1;
using ATMS.Project.Contracts.Commands.WorkGroups;
using ATMS.Project.Contracts.Models.WorkGroups;
using ATMS.Project.Contracts.Requests.WorkGroups;
using ATMS.Data.Criteria;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Project.API.Tests;

public class WorkGroupControllerTest : BaseControllerTest
{
    private readonly WorkGroupController _controller;

    public WorkGroupControllerTest()
    {
        _controller = new WorkGroupController(MediatorMock.Object);
    }

    [Fact]
    public async Task GetGroups_Returns200AndUsesProjectId()
    {
        var projectId = Guid.NewGuid();
        WorkGroupModel[] expected = [];
        MediatorMock
            .Setup(x => x.Send(
                It.Is<GetWorkGroupsRequest>(request => request.ProjectId == projectId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetGroups(projectId, CancellationToken.None);

        var response = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, response.Value);
    }

    [Fact]
    public async Task GetMilestones_OverridesRouteProjectIdAndPreservesSearchAndCursor()
    {
        var projectId = Guid.NewGuid();
        var request = new GetMilestonesRequest
        {
            ProjectId = Guid.NewGuid(),
            Search = "release",
            Cursor = "cursor",
            PageSize = 10
        };
        var expected = new KeysetPagedResult<MilestoneOptionModel> { PageSize = 10 };
        MediatorMock
            .Setup(mediator => mediator.Send(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetMilestones(projectId, request, CancellationToken.None);

        Assert.Equal(projectId, request.ProjectId);
        Assert.Equal("release", request.Search);
        Assert.Equal("cursor", request.Cursor);
        var response = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, response.Value);
    }

    [Fact]
    public async Task Create_OverridesBodyProjectIdWithRouteValueAndReturns201()
    {
        var projectId = Guid.NewGuid();
        var parentWorkGroupId = Guid.NewGuid();
        var id = Guid.NewGuid();
        var command = new CreateWorkGroupCommand
        {
            ProjectId = Guid.NewGuid(),
            Title = "Group",
            ParentWorkGroupId = parentWorkGroupId
        };
        MediatorMock
            .Setup(x => x.Send(
                command,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(id);

        var result = await _controller.Create(projectId, command, CancellationToken.None);

        Assert.Equal(projectId, command.ProjectId);
        var response = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(StatusCodes.Status201Created, response.StatusCode);
        Assert.Equal(id, response.Value);
        Assert.Equal(nameof(_controller.GetGroups), response.ActionName);
    }

    [Fact]
    public async Task Update_OverridesBodyIdsWithRouteValuesAndReturns204()
    {
        var projectId = Guid.NewGuid();
        var workGroupId = Guid.NewGuid();
        var command = new UpdateWorkGroupCommand
        {
            ProjectId = Guid.NewGuid(),
            WorkGroupId = Guid.NewGuid(),
            Title = "Updated"
        };

        var result = await _controller.Update(
            projectId,
            workGroupId,
            command,
            CancellationToken.None);

        Assert.Equal(projectId, command.ProjectId);
        Assert.Equal(workGroupId, command.WorkGroupId);
        Assert.IsType<NoContentResult>(result);
        MediatorMock.Verify(
            x => x.Send(command, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Delete_UsesRouteIdsAndReturns204()
    {
        var projectId = Guid.NewGuid();
        var workGroupId = Guid.NewGuid();

        var result = await _controller.Delete(
            projectId,
            workGroupId,
            CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        MediatorMock.Verify(
            x => x.Send(
                It.Is<DeleteWorkGroupCommand>(command =>
                    command.ProjectId == projectId && command.WorkGroupId == workGroupId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

using ATMS.Data.Criteria;
using ATMS.Project.API.Controllers.v1;
using ATMS.Project.Contracts.Commands.WorkProjects;
using ATMS.Project.Contracts.Models.WorkProjects;
using ATMS.Project.Contracts.Models.Users;
using ATMS.Project.Contracts.Requests.Users;
using ATMS.Project.Contracts.Requests.WorkProjects;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Project.API.Tests;

public class ProjectControllerTest : BaseControllerTest
{
    private readonly ProjectController _controller;

    public ProjectControllerTest()
    {
        _controller = new ProjectController(MediatorMock.Object);
    }

    [Fact]
    public async Task Index_Returns200WithPagedItems()
    {
        var request = new GetWorkProjectsRequest();
        var expected = new PagedResult<WorkProjectItemModel>
        {
            Items = [new WorkProjectItemModel()],
            TotalCount = 1,
            Page = 1,
            PageSize = 10
        };
        MediatorMock.Setup(x => x.Send(request, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var result = await _controller.Index(request, CancellationToken.None);

        var response = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(expected, response.Value);
    }

    [Fact]
    public async Task Get_Returns200WithDetailsModel()
    {
        var id = Guid.NewGuid();
        var expected = new WorkProjectModel();
        MediatorMock
            .Setup(x => x.Send(
                It.Is<GetWorkProjectRequest>(request => request.Id == id),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.Get(id, CancellationToken.None);

        var response = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(expected, response.Value);
    }

    [Fact]
    public async Task GetTeamMembers_Returns200WithCandidates()
    {
        var expected = new[] { new UserModel { Id = Guid.NewGuid() } };
        MediatorMock
            .Setup(x => x.Send(
                It.IsAny<GetProjectTeamMembersRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetTeamMembers(CancellationToken.None);

        var response = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(expected, response.Value);
    }

    [Fact]
    public async Task Create_Returns201WithGeneratedId()
    {
        var id = Guid.NewGuid();
        var command = CreateCommand();
        MediatorMock.Setup(x => x.Send(command, It.IsAny<CancellationToken>())).ReturnsAsync(id);

        var result = await _controller.Create(command, CancellationToken.None);

        var response = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(StatusCodes.Status201Created, response.StatusCode);
        Assert.Equal(id, response.Value);
        Assert.Equal(nameof(_controller.Get), response.ActionName);
    }

    [Fact]
    public async Task UpdateStatus_UsesRouteIdAndReturns204()
    {
        var id = Guid.NewGuid();
        var command = new UpdateWorkProjectStatusCommand { Id = Guid.NewGuid(), ProjectStatusId = 2 };

        var result = await _controller.UpdateStatus(id, command, CancellationToken.None);

        Assert.Equal(id, command.Id);
        Assert.IsType<NoContentResult>(result);
        MediatorMock.Verify(x => x.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_Returns204()
    {
        var id = Guid.NewGuid();

        var result = await _controller.Delete(id, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        MediatorMock.Verify(x => x.Send(
            It.Is<DeleteWorkProjectCommand>(command => command.Id == id),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private CreateWorkProjectCommand CreateCommand()
    {
        return new CreateWorkProjectCommand
        {
            Title = "Project",
            ProjectTypeId = 1,
            ProjectKindId = 1,
            ProjectStatusId = 1
        };
    }
}

using ATMS.Data.Criteria;
using ATMS.Project.API.Controllers.v1;
using ATMS.Project.Contracts.Commands.Comments;
using ATMS.Project.Contracts.Models.Comments;
using ATMS.Project.Contracts.Requests.Comments;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Project.API.Tests;

public sealed class CommentControllerTest : BaseControllerTest
{
    private readonly CommentController _controller;

    public CommentControllerTest()
    {
        _controller = new CommentController(MediatorMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    [Fact]
    public async Task GetMany_UsesRouteProjectAndReturnsThePage()
    {
        var projectId = Guid.NewGuid();
        var request = new GetCommentsRequest { WorkTaskId = Guid.NewGuid() };
        var page = new KeysetPagedResult<CommentModel>();
        MediatorMock.Setup(mediator => mediator.Send(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        var result = await _controller.GetMany(projectId, request, CancellationToken.None);

        Assert.Equal(projectId, request.ProjectId);
        Assert.Same(page, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task Create_UsesRouteProjectAndReturns201()
    {
        var projectId = Guid.NewGuid();
        var command = new CreateCommentCommand { WorkTaskId = Guid.NewGuid(), Text = "Hello" };
        var model = new CommentModel { Id = Guid.NewGuid(), CreatedBy = new() };
        MediatorMock.Setup(mediator => mediator.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(model);

        var result = await _controller.Create(projectId, command, CancellationToken.None);

        Assert.Equal(projectId, command.ProjectId);
        var created = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status201Created, created.StatusCode);
        Assert.Same(model, created.Value);
    }

    [Fact]
    public async Task Update_UsesRouteIdentifiers()
    {
        var projectId = Guid.NewGuid();
        var id = Guid.NewGuid();
        var command = new UpdateCommentCommand { Text = "Changed" };
        MediatorMock.Setup(mediator => mediator.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CommentModel { CreatedBy = new() });

        await _controller.Update(projectId, id, command, CancellationToken.None);

        Assert.Equal(projectId, command.ProjectId);
        Assert.Equal(id, command.CommentId);
    }

    [Fact]
    public async Task Delete_UsesRouteIdentifiersAndReturns204()
    {
        var projectId = Guid.NewGuid();
        var id = Guid.NewGuid();

        var result = await _controller.Delete(projectId, id, CancellationToken.None);

        MediatorMock.Verify(mediator => mediator.Send(
            It.Is<DeleteCommentCommand>(command => command.ProjectId == projectId && command.CommentId == id),
            It.IsAny<CancellationToken>()), Times.Once);
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Get_UsesRouteIdentifiers()
    {
        var projectId = Guid.NewGuid();
        var id = Guid.NewGuid();
        var workTaskId = Guid.NewGuid();
        var model = new CommentModel { Id = id, CreatedBy = new() };
        MediatorMock.Setup(mediator => mediator.Send(
                It.Is<GetCommentRequest>(request => request.ProjectId == projectId &&
                    request.CommentId == id && request.WorkTaskId == workTaskId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(model);

        var result = await _controller.Get(projectId, id, workTaskId, CancellationToken.None);

        Assert.Same(model, Assert.IsType<OkObjectResult>(result.Result).Value);
    }
}

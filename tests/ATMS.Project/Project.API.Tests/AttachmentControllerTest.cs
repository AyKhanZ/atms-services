using ATMS.Project.API.Controllers.v1;
using ATMS.Project.API.Results;
using ATMS.Project.Contracts.Commands.Attachments;
using ATMS.Project.Contracts.Models.Attachments;
using ATMS.Project.Contracts.Requests.Attachments;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Project.API.Tests;

public class AttachmentControllerTest : BaseControllerTest
{
    private readonly AttachmentController _controller;

    public AttachmentControllerTest()
    {
        _controller = new AttachmentController(MediatorMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private void Content(bool canPreview)
    {
        MediatorMock
            .Setup(mediator => mediator.Send(It.IsAny<GetAttachmentContentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AttachmentContentModel
            {
                FileName = "Отчёт.pdf",
                ContentType = "application/pdf",
                PhysicalPath = "/app/attachments/p/2026/09/file.pdf",
                CanPreview = canPreview
            });
    }

    [Fact]
    public async Task Upload_OverridesRouteIdsAndReturns201WithTheFile()
    {
        var projectId = Guid.NewGuid();
        var workTaskId = Guid.NewGuid();
        var command = new UploadAttachmentCommand { ProjectId = Guid.NewGuid(), WorkTaskId = Guid.NewGuid() };
        var model = new AttachmentModel { Id = Guid.NewGuid() };
        MediatorMock.Setup(mediator => mediator.Send(command, It.IsAny<CancellationToken>())).ReturnsAsync(model);

        var result = await _controller.Upload(projectId, workTaskId, command, CancellationToken.None);

        Assert.Equal(projectId, command.ProjectId);
        Assert.Equal(workTaskId, command.WorkTaskId);
        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(StatusCodes.Status201Created, created.StatusCode);
        Assert.Same(model, created.Value);
    }

    [Fact]
    public async Task GetContent_ReturnsTheStoredFileAsAnAttachmentContentResult()
    {
        Content(canPreview: true);

        var result = await _controller.GetContent(Guid.NewGuid(), Guid.NewGuid(), inline: true, CancellationToken.None);

        Assert.IsType<AttachmentContentResult>(result);
    }

    [Fact]
    public async Task Rename_OverridesRouteIdsAndReturns204()
    {
        var projectId = Guid.NewGuid();
        var attachmentId = Guid.NewGuid();
        var command = new RenameAttachmentCommand { FileName = "New" };

        var result = await _controller.Rename(projectId, attachmentId, command, CancellationToken.None);

        Assert.Equal(projectId, command.ProjectId);
        Assert.Equal(attachmentId, command.AttachmentId);
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_SendsTheScopedCommandAndReturns204()
    {
        var projectId = Guid.NewGuid();
        var attachmentId = Guid.NewGuid();

        var result = await _controller.Delete(projectId, attachmentId, CancellationToken.None);

        MediatorMock.Verify(mediator => mediator.Send(
            It.Is<DeleteAttachmentCommand>(command =>
                command.ProjectId == projectId && command.AttachmentId == attachmentId),
            It.IsAny<CancellationToken>()), Times.Once);
        Assert.IsType<NoContentResult>(result);
    }
}

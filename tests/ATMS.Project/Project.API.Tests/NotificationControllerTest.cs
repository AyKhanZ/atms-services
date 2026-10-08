using ATMS.Data.Criteria;
using ATMS.Project.API.Controllers.v1;
using ATMS.Project.Contracts.Commands.Notifications;
using ATMS.Project.Contracts.Models.Notifications;
using ATMS.Project.Contracts.Requests.Notifications;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Project.API.Tests;

public sealed class NotificationControllerTest : BaseControllerTest
{
    private readonly NotificationController _controller;

    public NotificationControllerTest()
    {
        _controller = new NotificationController(MediatorMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    [Fact]
    public async Task GetMany_ReturnsThePage()
    {
        var request = new GetNotificationsRequest { UnreadOnly = true };
        var page = new KeysetPagedResult<NotificationModel>();
        MediatorMock.Setup(mediator => mediator.Send(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        var result = await _controller.GetMany(request, CancellationToken.None);

        Assert.Same(page, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task GetSummary_ReturnsTheUnreadCount()
    {
        var summary = new NotificationSummaryModel { UnreadCount = 3 };
        MediatorMock.Setup(mediator => mediator.Send(It.IsAny<GetNotificationSummaryRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);

        var result = await _controller.GetSummary(CancellationToken.None);

        Assert.Same(summary, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task MarkRead_UsesRouteIdAndReturns204()
    {
        var id = Guid.NewGuid();

        var result = await _controller.MarkRead(id, CancellationToken.None);

        MediatorMock.Verify(mediator => mediator.Send(
            It.Is<MarkNotificationReadCommand>(command => command.NotificationId == id),
            It.IsAny<CancellationToken>()), Times.Once);
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task MarkUnread_UsesRouteIdAndReturns204()
    {
        var id = Guid.NewGuid();

        var result = await _controller.MarkUnread(id, CancellationToken.None);

        MediatorMock.Verify(mediator => mediator.Send(
            It.Is<MarkNotificationUnreadCommand>(command => command.NotificationId == id),
            It.IsAny<CancellationToken>()), Times.Once);
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task MarkAllRead_Returns204()
    {
        var result = await _controller.MarkAllRead(CancellationToken.None);

        MediatorMock.Verify(mediator => mediator.Send(
            It.IsAny<MarkAllNotificationsReadCommand>(),
            It.IsAny<CancellationToken>()), Times.Once);
        Assert.IsType<NoContentResult>(result);
    }
}

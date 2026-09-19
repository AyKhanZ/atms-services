using System.Reflection;
using ATMS.Application.Security;
using ATMS.Data.Criteria;
using ATMS.Data.Enums;
using ATMS.Project.API.Controllers.v1;
using ATMS.Project.Contracts.Commands.Search;
using ATMS.Project.Contracts.Models.Search;
using ATMS.Project.Contracts.Requests.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Project.API.Tests;

public class SearchControllerTest : BaseControllerTest
{
    [Fact]
    public async Task Get_ForwardsQueryAndCancellationAndReturnsGroupedResult()
    {
        var request = new GetGlobalSearchRequest { Q = "search", Take = 50 };
        var expected = new GlobalSearchModel();
        using var cancellation = new CancellationTokenSource();
        MediatorMock.Setup(mediator => mediator.Send(request, cancellation.Token)).ReturnsAsync(expected);
        var controller = new SearchController(MediatorMock.Object);

        var response = await controller.Get(request, cancellation.Token);

        Assert.Same(expected, Assert.IsType<OkObjectResult>(response.Result).Value);
        MediatorMock.Verify(mediator => mediator.Send(request, cancellation.Token), Times.Once);
    }

    [Fact]
    public async Task GetPage_TakesTheKindFromTheRouteAndReturnsAPage()
    {
        var request = new GetGlobalSearchPageRequest { Q = "search", PageSize = 20 };
        var expected = new KeysetPagedResult<GlobalSearchItemModel>();
        using var cancellation = new CancellationTokenSource();
        MediatorMock
            .Setup(mediator => mediator.Send(
                It.Is<GetGlobalSearchPageRequest>(value => value.ItemType == GlobalSearchItemType.Task && value.Q == "search"),
                cancellation.Token))
            .ReturnsAsync(expected);
        var controller = new SearchController(MediatorMock.Object);

        var response = await controller.GetPage(GlobalSearchItemType.Task, request, cancellation.Token);

        Assert.Same(expected, Assert.IsType<OkObjectResult>(response.Result).Value);
        MediatorMock.VerifyAll();
    }

    // The results page asks for GET search/{kind}. Without this route the handler exists but
    // nothing reaches it, and the page fails with a 404 that no handler test would ever see.
    [Fact]
    public void GetPage_IsServedUnderTheSearchRouteWithTheKindInThePath()
    {
        var method = typeof(SearchController).GetMethod(nameof(SearchController.GetPage))!;

        Assert.Equal("{itemType}", Assert.Single(method.GetCustomAttributes<HttpGetAttribute>()).Template);
    }

    [Fact]
    public async Task RecordRecent_UsesRouteValuesAndReturns204()
    {
        var id = Guid.NewGuid();
        using var cancellation = new CancellationTokenSource();
        MediatorMock.Setup(mediator => mediator.Send(
                It.Is<RecordGlobalSearchRecentCommand>(command => command.ItemId == id && command.ItemType == (int)GlobalSearchItemType.Subtask),
                cancellation.Token))
            .Returns(Task.CompletedTask);
        var controller = new SearchController(MediatorMock.Object);

        var result = await controller.RecordRecent((int)GlobalSearchItemType.Subtask, id, cancellation.Token);

        Assert.IsType<NoContentResult>(result);
        MediatorMock.VerifyAll();
    }

    [Fact]
    public void Search_RequiresAuthenticationAndProjectViewPermission()
    {
        Assert.True(typeof(SearchController).IsDefined(typeof(AuthorizeAttribute), true));
        foreach (var requestType in new[]
                 {
                     typeof(GetGlobalSearchRequest), typeof(GetGlobalSearchPageRequest), typeof(RecordGlobalSearchRecentCommand)
                 })
        {
            var access = Assert.Single(requestType.GetCustomAttributes(typeof(AccessAttribute), false).Cast<AccessAttribute>());
            Assert.Contains(PermissionEnum.ProjectView, access.Permissions);
        }
    }
}


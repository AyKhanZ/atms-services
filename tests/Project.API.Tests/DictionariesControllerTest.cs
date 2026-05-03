using ATMS.Application.Models;
using ATMS.Project.API.Controllers.v1;
using ATMS.Project.Contracts.Requests.Dictionaries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Project.API.Tests;

public class DictionariesControllerTest : BaseControllerTest
{
    private readonly DictionaryController _controller;

    public DictionariesControllerTest()
    {
        _controller = new DictionaryController(MediatorMock.Object);
    }

    [Theory]
    [MemberData(nameof(GetEndpointTestCases))]
    public async Task Endpoint_Returns200_WithResult(
        Func<DictionaryController, CancellationToken, Task<ActionResult<IReadOnlyList<DictionaryModel>>>> action,
        Type requestType)
    {
        var expected = new DictionaryModel[] { new(), new() };

        MediatorMock
            .Setup(m => m.Send(It.Is<IRequest<DictionaryModel[]>>(r => r.GetType() == requestType),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await action(_controller, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);
    }

    public static TheoryData<Func<DictionaryController, CancellationToken, Task<ActionResult<IReadOnlyList<DictionaryModel>>>>, Type> GetEndpointTestCases() => new()
    {
        { (c, ct) => c.GetProjectTypes(ct), typeof(GetProjectTypeDictionariesRequest) },
        { (c, ct) => c.GetProjectKinds(ct), typeof(GetProjectKindDictionariesRequest) },
        { (c, ct) => c.GetProjectStatuses(ct), typeof(GetProjectStatusDictionariesRequest) },
        { (c, ct) => c.GetWorkTicketStatuses(ct), typeof(GetWorkTicketStatusDictionariesRequest) },
        { (c, ct) => c.GetWorkTicketTypes(ct), typeof(GetWorkTicketTypeDictionariesRequest) },
        { (c, ct) => c.GetWorkTaskStatuses(ct), typeof(GetWorkTaskStatusDictionariesRequest) },
        { (c, ct) => c.GetWorkItemPriorities(ct), typeof(GetWorkItemPriorityDictionariesRequest) },
        { (c, ct) => c.GetWorkGroupStatuses(ct), typeof(GetWorkGroupStatusDictionariesRequest) },
    };
}
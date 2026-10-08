using ATMS.Admin.API.Controllers.v1;
using ATMS.Admin.Contracts.Models.Dictionaries;
using ATMS.Admin.Contracts.Requests.Dictionaries;
using ATMS.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Admin.API.Tests;

public class DictionariesControllerTest
{
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly DictionaryController _controller;

    public DictionariesControllerTest()
    {
        _controller = new DictionaryController(_mediatorMock.Object);
    }

    [Fact]
    public async Task GetGenders_Returns200_WithResult()
    {
        var expected = new DictionaryModel[] { new(), new() };
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetGenderDictionariesRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetGenders(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);
        Assert.Equal(expected, ok.Value);
    }

    [Fact]
    public async Task GetMaritalStatuses_Returns200_WithResult()
    {
        var expected = new DictionaryModel[] { new(), new() };
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetMaritalStatusDictionariesRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetMaritalStatuses(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);
        Assert.Equal(expected, ok.Value);
    }

    [Fact]
    public async Task GetUserStatuses_Returns200_WithResult()
    {
        var expected = new DictionaryModel[] { new(), new() };
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetUserStatusDictionariesRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetUserStatuses(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);
    }

    [Fact]
    public async Task GetPermissions_Returns200_WithResult()
    {
        var expected = new PermissionModel[] { new(), new() };
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetPermissionDictionariesRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetPermissions(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);
        Assert.Equal(expected, ok.Value);
    }
}
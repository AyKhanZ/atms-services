using ATMS.Application.Models;
using ATMS.Project.Contracts.Requests.Dictionaries;
using ATMS.Project.Services.Handlers.Dictionaries;
using Moq;

namespace Project.Services.Tests.Handlers.Dictionaries;

public class GetWorkTaskStatusDictionariesHandlerTest : BaseHandlerTest
{
    private readonly GetWorkTaskStatusDictionariesHandler _handler;

    public GetWorkTaskStatusDictionariesHandlerTest()
    {
        _handler = new GetWorkTaskStatusDictionariesHandler(
            DictionariesRepositoryMock.Object,
            CacheServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsMappedWorkTaskStatuses()
    {
        SetupCacheMiss<DictionaryModel[]>();
        DictionariesRepositoryMock
            .Setup(r => r.GetWorkTaskStatusesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _handler.Handle(new GetWorkTaskStatusDictionariesRequest(), CancellationToken.None);

        Assert.NotNull(result);
        DictionariesRepositoryMock.Verify(r => r.GetWorkTaskStatusesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCacheHit_DoesNotCallRepository()
    {
        var expected = new DictionaryModel[] { new(), new() };
        SetupCacheHit(expected);

        var result = await _handler.Handle(new GetWorkTaskStatusDictionariesRequest(), CancellationToken.None);

        Assert.Equal(expected, result);
        DictionariesRepositoryMock.Verify(
            r => r.GetWorkTaskStatusesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRepositoryReturnsEmpty_ReturnsEmptyArray()
    {
        SetupCacheMiss<DictionaryModel[]>();
        DictionariesRepositoryMock
            .Setup(r => r.GetWorkTaskStatusesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _handler.Handle(new GetWorkTaskStatusDictionariesRequest(), CancellationToken.None);

        Assert.Empty(result);
    }
}
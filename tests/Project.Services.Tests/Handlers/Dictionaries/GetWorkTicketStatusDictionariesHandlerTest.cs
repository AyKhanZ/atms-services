using ATMS.Application.Models;
using ATMS.Project.Contracts.Requests.Dictionaries;
using ATMS.Project.Services.Handlers.Dictionaries;
using Moq;

namespace Project.Services.Tests.Handlers.Dictionaries;

public class GetWorkTicketStatusDictionariesHandlerTest : BaseHandlerTest
{
    private readonly GetWorkTicketStatusDictionariesHandler _handler;

    public GetWorkTicketStatusDictionariesHandlerTest()
    {
        _handler = new GetWorkTicketStatusDictionariesHandler(
            DictionariesRepositoryMock.Object,
            CacheServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsMappedWorkTicketStatuses()
    {
        SetupCacheMiss<DictionaryModel[]>();
        DictionariesRepositoryMock
            .Setup(r => r.GetWorkTicketStatusesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _handler.Handle(new GetWorkTicketStatusDictionariesRequest(), CancellationToken.None);

        Assert.NotNull(result);
        DictionariesRepositoryMock.Verify(r => r.GetWorkTicketStatusesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCacheHit_DoesNotCallRepository()
    {
        var expected = new DictionaryModel[] { new(), new() };
        SetupCacheHit(expected);

        var result = await _handler.Handle(new GetWorkTicketStatusDictionariesRequest(), CancellationToken.None);

        Assert.Equal(expected, result);
        DictionariesRepositoryMock.Verify(
            r => r.GetWorkTicketStatusesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRepositoryReturnsEmpty_ReturnsEmptyArray()
    {
        SetupCacheMiss<DictionaryModel[]>();
        DictionariesRepositoryMock
            .Setup(r => r.GetWorkTicketStatusesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _handler.Handle(new GetWorkTicketStatusDictionariesRequest(), CancellationToken.None);

        Assert.Empty(result);
    }
}
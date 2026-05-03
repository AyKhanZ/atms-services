using ATMS.Application.Models;
using ATMS.Project.Contracts.Requests.Dictionaries;
using ATMS.Project.Services.Handlers.Dictionaries;
using Moq;

namespace Project.Services.Tests.Handlers.Dictionaries;

public class GetProjectKindDictionariesHandlerTest : BaseHandlerTest
{
    private readonly GetProjectKindDictionariesHandler _handler;

    public GetProjectKindDictionariesHandlerTest()
    {
        _handler = new GetProjectKindDictionariesHandler(
            DictionariesRepositoryMock.Object,
            CacheServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsMappedProjectKinds()
    {
        SetupCacheMiss<DictionaryModel[]>();
        DictionariesRepositoryMock
            .Setup(r => r.GetProjectKindsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _handler.Handle(new GetProjectKindDictionariesRequest(), CancellationToken.None);

        Assert.NotNull(result);
        DictionariesRepositoryMock.Verify(r => r.GetProjectKindsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCacheHit_DoesNotCallRepository()
    {
        var expected = new DictionaryModel[] { new(), new() };
        SetupCacheHit(expected);

        var result = await _handler.Handle(new GetProjectKindDictionariesRequest(), CancellationToken.None);

        Assert.Equal(expected, result);
        DictionariesRepositoryMock.Verify(
            r => r.GetProjectKindsAsync(It.IsAny<CancellationToken>()),
            Times.Never); // cache hit — repository never called
    }

    [Fact]
    public async Task Handle_WhenRepositoryReturnsEmpty_ReturnsEmptyArray()
    {
        SetupCacheMiss<DictionaryModel[]>();
        DictionariesRepositoryMock
            .Setup(r => r.GetProjectKindsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _handler.Handle(new GetProjectKindDictionariesRequest(), CancellationToken.None);

        Assert.Empty(result);
    }
}
using ATMS.Application.Models;
using ATMS.Project.Contracts.Requests.Dictionaries;
using ATMS.Project.Services.Handlers.Dictionaries;
using Moq;

namespace Project.Services.Tests.Handlers.Dictionaries;

public class GetProjectTypeDictionariesHandlerTest : BaseHandlerTest
{
    private readonly GetProjectTypeDictionariesHandler _handler;

    public GetProjectTypeDictionariesHandlerTest()
    {
        _handler = new GetProjectTypeDictionariesHandler(
            DictionariesRepositoryMock.Object,
            CacheServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsMappedProjectTypes()
    {
        SetupCacheMiss<DictionaryModel[]>();
        DictionariesRepositoryMock
            .Setup(r => r.GetProjectTypesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _handler.Handle(new GetProjectTypeDictionariesRequest(), CancellationToken.None);

        Assert.NotNull(result);
        DictionariesRepositoryMock.Verify(r => r.GetProjectTypesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCacheHit_DoesNotCallRepository()
    {
        var expected = new DictionaryModel[] { new(), new() };
        SetupCacheHit(expected);

        var result = await _handler.Handle(new GetProjectTypeDictionariesRequest(), CancellationToken.None);

        Assert.Equal(expected, result);
        DictionariesRepositoryMock.Verify(
            r => r.GetProjectTypesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRepositoryReturnsEmpty_ReturnsEmptyArray()
    {
        SetupCacheMiss<DictionaryModel[]>();
        DictionariesRepositoryMock
            .Setup(r => r.GetProjectTypesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _handler.Handle(new GetProjectTypeDictionariesRequest(), CancellationToken.None);

        Assert.Empty(result);
    }
}
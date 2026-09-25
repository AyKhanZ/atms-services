using System.Globalization;
using ATMS.Application.Models;
using ATMS.Caching.Constants;
using ATMS.Caching.Services.Interfaces;
using ATMS.Project.Data.Entities.Dictionaries;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Dictionaries;
using Moq;

namespace Project.Services.Tests.Dictionaries;

public sealed class DictionaryCacheServiceTest
{
    private readonly Mock<IDictionariesRepository> _repository = new();
    private readonly Mock<ICacheService> _cache = new();

    private DictionaryCacheService Service() => new(_repository.Object, _cache.Object);

    [Theory]
    [InlineData("en")]
    [InlineData("ru")]
    [InlineData("az")]
    public async Task GetWorkTaskStatusesAsync_KeysTheCacheByTheCallersLanguage(string culture)
    {
        _cache
            .Setup(cache => cache.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<DictionaryModel[]>>>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        await InCultureAsync(culture, () => Service().GetWorkTaskStatusesAsync(CancellationToken.None));

        _cache.Verify(cache => cache.GetOrSetAsync(
            CacheKeys.Project.AllWorkTaskStatuses(culture),
            It.IsAny<Func<Task<DictionaryModel[]>>>(),
            CacheTtl.Dictionary,
            It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task GetWorkTaskStatusesAsync_OnAMiss_TranslatesWhatTheRepositoryHolds()
    {
        _cache
            .Setup(cache => cache.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<DictionaryModel[]>>>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<Task<DictionaryModel[]>>, TimeSpan, CancellationToken>(
                async (_, factory, _, _) => await factory());
        _repository
            .Setup(repository => repository.GetWorkTaskStatusesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new WorkTaskStatus
                {
                    Id = 3,
                    Code = "Done",
                    Translations =
                    [
                        new WorkTaskStatusTranslation { Language = "en", Name = "Done" },
                        new WorkTaskStatusTranslation { Language = "ru", Name = "Готово" }
                    ]
                }
            ]);

        var statuses = await InCultureAsync("ru", () => Service().GetWorkTaskStatusesAsync(CancellationToken.None));

        var status = Assert.Single(statuses);
        Assert.Equal(3, status.Id);
        Assert.Equal("Done", status.Code);
        Assert.Equal("Готово", status.Name);
    }

    [Fact]
    public async Task GetWorkTaskStatusesAsync_OnAHit_DoesNotReadTheDatabase()
    {
        DictionaryModel[] cached = [new() { Id = 1, Code = "New", Name = "New" }];
        _cache
            .Setup(cache => cache.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<DictionaryModel[]>>>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cached);

        var statuses = await Service().GetWorkTaskStatusesAsync(CancellationToken.None);

        Assert.Same(cached, statuses);
        _repository.Verify(
            repository => repository.GetWorkTaskStatusesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static async Task<T> InCultureAsync<T>(string culture, Func<Task<T>> action)
    {
        var previous = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = new CultureInfo(culture);
        try
        {
            return await action();
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }
}

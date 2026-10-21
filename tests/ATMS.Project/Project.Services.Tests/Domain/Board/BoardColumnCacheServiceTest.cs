using System.Globalization;
using ATMS.Application.Localization;
using ATMS.Application.Models;
using ATMS.Caching.Constants;
using ATMS.Caching.Services.Interfaces;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Models.BoardColumns;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Domain.Board;
using ATMS.Project.Services.Domain.Dictionaries.Interfaces;
using Moq;

namespace Project.Services.Tests.Domain.Board;

public sealed class BoardColumnCacheServiceTest
{
    private readonly Mock<IBoardColumnRepository> _columns = new();
    private readonly Mock<IDictionaryCacheService> _dictionaries = new();
    private readonly Mock<ICacheService> _cache = new();

    public BoardColumnCacheServiceTest()
    {
        _dictionaries
            .Setup(dictionaries => dictionaries.GetWorkTaskStatusesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new DictionaryModel { Id = (int)WorkTaskStatusEnum.New, Code = "New", Name = "Новый" },
                new DictionaryModel { Id = (int)WorkTaskStatusEnum.InProgress, Code = "InProgress", Name = "В работе" }
            ]);
        _cache
            .Setup(cache => cache.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<BoardColumnModel[]>>>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<Task<BoardColumnModel[]>>, TimeSpan, CancellationToken>(
                async (_, factory, _, _) => await factory());
    }

    [Fact]
    public async Task GetAllAsync_OnAMiss_StoresTheOrderedListForTheCallersLanguage()
    {
        _columns
            .Setup(repository => repository.GetOrderedAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([Column(1, "New", (int)WorkTaskStatusEnum.New, "Новый")]);

        var result = await InCultureAsync("ru", () => Service().GetAllAsync(CancellationToken.None));

        var model = Assert.Single(result);
        Assert.Equal("Новый", model.Name);
        Assert.Equal((int)WorkTaskStatusEnum.New, model.Category);
        _cache.Verify(cache => cache.GetOrSetAsync(
            CacheKeys.Project.AllBoardColumns(SupportedLanguages.Russian),
            It.IsAny<Func<Task<BoardColumnModel[]>>>(),
            CacheTtl.Dictionary,
            It.IsAny<CancellationToken>()));
    }

    // a status split into Doing and Done names its parts with the status, a single column is its own name
    [Fact]
    public async Task GetAllAsync_LabelsSplitColumnsWithTheirStatus()
    {
        _columns
            .Setup(repository => repository.GetOrderedAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                Column(1, "New", (int)WorkTaskStatusEnum.New, "Новый"),
                Column(2, "InProgressDoing", (int)WorkTaskStatusEnum.InProgress, "В процессе"),
                Column(4, "InProgressDone", (int)WorkTaskStatusEnum.InProgress, "Готово")
            ]);

        var result = await InCultureAsync("ru", () => Service().GetAllAsync(CancellationToken.None));

        Assert.Equal(["Новый", "В работе · В процессе", "В работе · Готово"], result.Select(column => column.Label));
        Assert.Equal(["Новый", "В процессе", "Готово"], result.Select(column => column.Name));
    }

    [Fact]
    public async Task GetAllAsync_OnAHit_DoesNotReadTheDatabase()
    {
        _cache
            .Setup(cache => cache.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<BoardColumnModel[]>>>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await Service().GetAllAsync(CancellationToken.None);

        Assert.Empty(result);
        _columns.Verify(repository => repository.GetOrderedAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private BoardColumnCacheService Service() => new(_columns.Object, _dictionaries.Object, _cache.Object);

    private static BoardColumn Column(int id, string code, int category, string russian) =>
        new()
        {
            Id = id,
            Code = code,
            Category = category,
            Order = id,
            Translations = [new BoardColumnTranslation { Language = SupportedLanguages.Russian, Name = russian }]
        };

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

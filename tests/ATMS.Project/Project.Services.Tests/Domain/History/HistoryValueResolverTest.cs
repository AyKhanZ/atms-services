using ATMS.Project.Contracts.Models.BoardColumns;
using ATMS.Project.Services.Domain.Board.Interfaces;
using ATMS.Application.Models;
using ATMS.Caching.Services.Interfaces;
using ATMS.Project.Services.Domain.Dictionaries;
using System.Globalization;
using ATMS.Data.Enums;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Entities.Dictionaries;
using ATMS.Project.Data.Models.History;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Domain.History;
using ATMS.Project.Services.Modules;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Project.Services.Tests.Domain.History;

public sealed class HistoryValueResolverTest
{
    private readonly Mock<IHistoryRepository> _historyRepository = new();
    private readonly Mock<IDictionariesRepository> _dictionariesRepository = new();

    public HistoryValueResolverTest()
    {
        _historyRepository
            .Setup(repository => repository.GetUsersAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _dictionariesRepository
            .Setup(repository => repository.GetWorkTaskStatusesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                Status(1, "New", "Новая"),
                Status(2, "InProgress", "В работе"),
                Status(3, "Done", "Готово")
            ]);
    }

    [Fact]
    // a bare "Готово" would not say which status it belongs to
    public async Task ResolveEntriesAsync_BoardColumn_ShowsTheColumnWithItsStatus()
    {
        var column = new BoardColumnModel
        {
            Id = 4,
            Code = "InProgressDone",
            Name = "Готово",
            Label = "В работе · Готово",
            Category = (int)WorkTaskStatusEnum.InProgress
        };

        var entry = Entry(HistoryEntityTypeEnum.WorkTask, Change(HistoryFieldEnum.BoardColumn, "2", "4"));

        var model = await Resolver([column]).ResolveEntriesAsync([entry], CancellationToken.None);

        var change = Assert.Single(Assert.Single(model).Changes);
        Assert.Equal("В работе · Готово", change.NewValue!.Name);
        Assert.Equal("InProgressDone", change.NewValue.Code);
    }

    private static readonly BoardColumnModel[] Columns =
    [
        new() { Id = 1, Code = "New", Name = "Новый", Label = "Новый", Category = (int)WorkTaskStatusEnum.New },
        new() { Id = 2, Code = "InProgressDoing", Name = "В процессе", Label = "В работе · В процессе", Category = (int)WorkTaskStatusEnum.InProgress },
        new() { Id = 4, Code = "InProgressDone", Name = "Готово", Label = "В работе · Готово", Category = (int)WorkTaskStatusEnum.InProgress },
        new() { Id = 3, Code = "Done", Name = "Выполнено", Label = "Выполнено", Category = (int)WorkTaskStatusEnum.Done }
    ];

    // "Статус: В работе → Выполнено" says it all, a column row next to it only confused
    [Theory]
    [InlineData("2", "3", "2", "3")]
    [InlineData("1", "2", "1", "2")]
    public async Task ResolveEntriesAsync_StatusIntoItsDefaultColumn_ShowsOnlyTheStatus(
        string fromStatus, string toStatus, string fromColumn, string toColumn)
    {
        var entry = Entry(
            HistoryEntityTypeEnum.WorkTask,
            Change(HistoryFieldEnum.Status, fromStatus, toStatus),
            Change(HistoryFieldEnum.BoardColumn, fromColumn, toColumn));

        var model = await Resolver(Columns).ResolveEntriesAsync([entry], CancellationToken.None);

        var change = Assert.Single(Assert.Single(model).Changes);
        Assert.Equal((int)HistoryFieldEnum.Status, change.Field);
    }

    // Doing to Done changes no status: the column row is the only news
    [Fact]
    public async Task ResolveEntriesAsync_MoveInsideInProgress_KeepsTheColumnRow()
    {
        var entry = Entry(HistoryEntityTypeEnum.WorkTask, Change(HistoryFieldEnum.BoardColumn, "2", "4"));

        var model = await Resolver(Columns).ResolveEntriesAsync([entry], CancellationToken.None);

        var change = Assert.Single(Assert.Single(model).Changes);
        Assert.Equal("В работе · В процессе", change.OldValue!.Name);
        Assert.Equal("В работе · Готово", change.NewValue!.Name);
    }

    // New to In Progress · Done: the status says In Progress, the column adds that the part is done
    [Fact]
    public async Task ResolveEntriesAsync_StatusAndPartOfAStatus_KeepsTheColumnRow()
    {
        var entry = Entry(
            HistoryEntityTypeEnum.WorkTask,
            Change(HistoryFieldEnum.Status, "1", "2"),
            Change(HistoryFieldEnum.BoardColumn, "1", "4"));

        var model = await Resolver(Columns).ResolveEntriesAsync([entry], CancellationToken.None);

        Assert.Equal(2, Assert.Single(model).Changes.Count);
    }

    private HistoryValueResolver Resolver(BoardColumnModel[]? columns = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMapperServices();
        var mapper = services.BuildServiceProvider().GetRequiredService<IMapper>();

        // The dictionary cache always misses here, so the statuses come translated from the repository.
        var cache = new Mock<ICacheService>();
        cache
            .Setup(service => service.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<DictionaryModel[]>>>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<Task<DictionaryModel[]>>, TimeSpan, CancellationToken>(
                async (_, factory, _, _) => await factory());
        var dictionaries = new DictionaryCacheService(_dictionariesRepository.Object, cache.Object);
        var boardColumns = new Mock<IBoardColumnCacheService>();
        boardColumns
            .Setup(service => service.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(columns ?? []);

        return new HistoryValueResolver(_historyRepository.Object, dictionaries, boardColumns.Object, mapper);
    }

    [Fact]
    public async Task ResolveEntriesAsync_Status_ReadsInTheCallersLanguageWithItsCode()
    {
        var entry = Entry(HistoryEntityTypeEnum.WorkTask, Change(HistoryFieldEnum.Status, "1", "2"));

        var model = await InCultureAsync("ru", () => Resolver().ResolveEntriesAsync([entry], CancellationToken.None));

        var change = Assert.Single(Assert.Single(model).Changes);
        Assert.Equal((int)HistoryFieldEnum.Status, change.Field);
        Assert.Equal("Новая", change.OldValue!.Name);
        Assert.Equal("InProgress", change.NewValue!.Code);
        Assert.Equal("2", change.NewValue.Id);
    }

    [Fact]
    public async Task ResolveEntriesAsync_AuthorAndAssignee_ComeWithNameAndAvatar()
    {
        var authorId = Guid.NewGuid();
        var participantId = Guid.NewGuid();
        _historyRepository
            .Setup(repository => repository.GetUsersAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, HistoryPerson>
            {
                [authorId] = new(authorId, "Rustam", "Aliyev", "users/rustam.png")
            });
        _historyRepository
            .Setup(repository => repository.GetParticipantsAsync(
                It.Is<IReadOnlyCollection<Guid>>(ids => ids.Contains(participantId)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, HistoryPerson>
            {
                [participantId] = new(participantId, "Dilara", "Zeynalova", null)
            });
        var entry = Entry(HistoryEntityTypeEnum.WorkTask, Change(HistoryFieldEnum.Assignee, null, participantId.ToString()));
        entry.CreatedById = authorId;

        var model = Assert.Single(await Resolver().ResolveEntriesAsync([entry], CancellationToken.None));

        Assert.Equal("Rustam", model.CreatedBy!.Name);
        Assert.Equal("users/rustam.png", model.CreatedBy.AvatarPath);
        var change = Assert.Single(model.Changes);
        Assert.Null(change.OldValue);
        Assert.Equal("Dilara Zeynalova", change.NewValue!.Name);
        Assert.Equal("Zeynalova", change.NewValue.Person!.Surname);
    }

    [Fact]
    public async Task ResolveEntriesAsync_ReferenceThatIsGone_HasNoName()
    {
        _historyRepository
            .Setup(repository => repository.GetWorkTicketsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        var ticketId = Guid.NewGuid().ToString();
        var entry = Entry(HistoryEntityTypeEnum.WorkTask, Change(HistoryFieldEnum.WorkTicket, null, ticketId));

        var change = Assert.Single(Assert.Single(await Resolver().ResolveEntriesAsync([entry], CancellationToken.None)).Changes);

        Assert.Equal(ticketId, change.NewValue!.Id);
        Assert.Null(change.NewValue.Name);
    }

    [Fact]
    public async Task ResolveEntriesAsync_TextAndDates_AreShownAsWritten()
    {
        var entry = Entry(
            HistoryEntityTypeEnum.WorkTask,
            Change(HistoryFieldEnum.Title, "Old title", "New title"),
            Change(HistoryFieldEnum.Deadline, null, "2026-09-30T00:00:00.0000000Z"));

        var changes = Assert.Single(await Resolver().ResolveEntriesAsync([entry], CancellationToken.None)).Changes;

        Assert.Contains(changes, change => change.OldValue?.Name == "Old title" && change.NewValue?.Name == "New title");
        Assert.Contains(changes, change => change.NewValue?.Name == "2026-09-30T00:00:00.0000000Z");
    }

    [Fact]
    public async Task ResolveEntriesAsync_MilestoneEntry_NamesItsSubject()
    {
        var milestoneId = Guid.NewGuid();
        _historyRepository
            .Setup(repository => repository.GetWorkGroupsAsync(
                It.Is<IReadOnlyCollection<Guid>>(ids => ids.Contains(milestoneId)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, HistoryReference>
            {
                [milestoneId] = new(milestoneId, null, "Sprint 1")
            });
        var entry = Entry(HistoryEntityTypeEnum.Milestone);
        entry.EntityId = milestoneId;
        entry.Action = (int)HistoryActionEnum.Deleted;

        var model = Assert.Single(await Resolver().ResolveEntriesAsync([entry], CancellationToken.None));

        Assert.Equal("Sprint 1", model.Subject!.Name);
        Assert.Equal((int)HistoryActionEnum.Deleted, model.Action);
    }

    [Fact]
    public async Task ResolveEntriesAsync_PageWithoutStatuses_DoesNotReadThem()
    {
        var entry = Entry(HistoryEntityTypeEnum.WorkTask, Change(HistoryFieldEnum.Title, "a", "b"));

        await Resolver().ResolveEntriesAsync([entry], CancellationToken.None);

        _dictionariesRepository.Verify(
            repository => repository.GetWorkTaskStatusesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ResolveStatesAsync_FromCreation_ListsEveryStatusWithItsDate()
    {
        var created = new DateTime(2026, 9, 1, 9, 0, 0, DateTimeKind.Utc);
        var started = created.AddDays(2);

        var states = (await Resolver().ResolveStatesAsync(
            HistoryEntityTypeEnum.WorkTask,
            [new HistoryStatusChange(created, null, null, "1"), new HistoryStatusChange(started, null, "1", "2")],
            new HistoryStatusChange(created, null, null, null),
            CancellationToken.None)).ToArray();

        Assert.Equal(["New", "InProgress"], states.Select(state => state.Status.Code));
        Assert.Equal([created, started], states.Select(state => state.ChangedAt));
    }

    /* A project created before the history showed its first status as "Earlier", without a date,
       although the migration had recorded when and by whom it was created. */
    [Fact]
    public async Task ResolveStatesAsync_ItemFromBeforeTheHistory_DatesItsFirstStatusByTheCreation()
    {
        var authorId = Guid.NewGuid();
        var created = new DateTime(2026, 8, 29, 9, 0, 0, DateTimeKind.Utc);
        var started = new DateTime(2026, 9, 3, 9, 0, 0, DateTimeKind.Utc);
        _historyRepository
            .Setup(repository => repository.GetUsersAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, HistoryPerson>
            {
                [authorId] = new(authorId, "Tatyana", "Raik", null)
            });

        var states = (await Resolver().ResolveStatesAsync(
            HistoryEntityTypeEnum.WorkTask,
            [new HistoryStatusChange(started, null, "1", "3")],
            new HistoryStatusChange(created, authorId, null, null),
            CancellationToken.None)).ToArray();

        Assert.Equal(["New", "Done"], states.Select(state => state.Status.Code));
        Assert.Equal(created, states[0].ChangedAt);
        Assert.Equal("Tatyana", states[0].ChangedBy!.Name);
        Assert.Equal(started, states[1].ChangedAt);
    }

    [Fact]
    public async Task ResolveStatesAsync_ListCutAtTheLimit_StartsWithoutADate()
    {
        var started = new DateTime(2026, 9, 3, 9, 0, 0, DateTimeKind.Utc);

        var states = (await Resolver().ResolveStatesAsync(
            HistoryEntityTypeEnum.WorkTask,
            [new HistoryStatusChange(started, null, "1", "3")],
            null,
            CancellationToken.None)).ToArray();

        Assert.Null(states[0].ChangedAt);
        Assert.Null(states[0].ChangedBy);
    }

    private static HistoryEntry Entry(HistoryEntityTypeEnum entityType, params HistoryChange[] changes) =>
        new()
        {
            Id = Guid.NewGuid(),
            EntityType = (int)entityType,
            EntityId = Guid.NewGuid(),
            WorkProjectId = Guid.NewGuid(),
            Action = (int)HistoryActionEnum.Updated,
            CreatedAt = DateTime.UtcNow,
            Changes = changes
        };

    private static HistoryChange Change(HistoryFieldEnum field, string? oldValue, string? newValue) =>
        new() { Id = Guid.NewGuid(), Field = (int)field, OldValue = oldValue, NewValue = newValue };

    private static WorkTaskStatus Status(int id, string code, string russian) =>
        new()
        {
            Id = id,
            Code = code,
            Translations =
            [
                new WorkTaskStatusTranslation { Language = "en", Name = code },
                new WorkTaskStatusTranslation { Language = "ru", Name = russian }
            ]
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

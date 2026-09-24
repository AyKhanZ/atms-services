using System.Data.Common;
using ATMS.Data.Criteria;
using ATMS.Data.Enums;
using ATMS.Project.Data.Criteria.History;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Project.Services.Tests.Repositories;

public class HistoryRepositoryTest
{
    private static readonly Guid[] Ids = [Guid.NewGuid()];

    public static TheoryData<string, Func<HistoryRepository, Task>> Queries => new()
    {
        {
            "GetManyAsync of a project",
            repository => repository.GetManyAsync(
                new HistoryOfProjectCriteria(Guid.NewGuid()),
                new KeysetPaginationCriteria<HistoryEntry>(null, 20, SortDirectionEnum.Desc),
                CancellationToken.None)
        },
        {
            "GetManyAsync of a task",
            repository => repository.GetManyAsync(
                new HistoryOfEntityCriteria(HistoryEntityTypeEnum.WorkTask, Guid.NewGuid()),
                new KeysetPaginationCriteria<HistoryEntry>(null, 20, SortDirectionEnum.Desc),
                CancellationToken.None)
        },
        {
            nameof(HistoryRepository.GetStatusChangesAsync),
            repository => repository.GetStatusChangesAsync(HistoryEntityTypeEnum.WorkTicket, Guid.NewGuid(), 200, CancellationToken.None)
        },
        {
            nameof(HistoryRepository.GetCreationAsync),
            repository => repository.GetCreationAsync(HistoryEntityTypeEnum.Project, Guid.NewGuid(), CancellationToken.None)
        },
        { nameof(HistoryRepository.GetUsersAsync), repository => repository.GetUsersAsync(Ids, CancellationToken.None) },
        { nameof(HistoryRepository.GetParticipantsAsync), repository => repository.GetParticipantsAsync(Ids, CancellationToken.None) },
        { nameof(HistoryRepository.GetWorkGroupsAsync), repository => repository.GetWorkGroupsAsync(Ids, CancellationToken.None) },
        { nameof(HistoryRepository.GetWorkTicketsAsync), repository => repository.GetWorkTicketsAsync(Ids, CancellationToken.None) },
        { nameof(HistoryRepository.GetWorkTasksAsync), repository => repository.GetWorkTasksAsync(Ids, CancellationToken.None) },
        { nameof(HistoryRepository.GetOrganizationsAsync), repository => repository.GetOrganizationsAsync(Ids, CancellationToken.None) },
        { nameof(HistoryRepository.GetRolesAsync), repository => repository.GetRolesAsync(Ids, CancellationToken.None) }
    };

    // Compiles the real query for PostgreSQL without connecting: a projection EF cannot translate
    // fails here instead of on the first request.
    [Theory]
    [MemberData(nameof(Queries))]
    public async Task Query_TranslatesToPostgreSql(string name, Func<HistoryRepository, Task> query)
    {
        var options = new DbContextOptionsBuilder<ProjectDbContext>()
            .UseNpgsql("Host=localhost;Database=query_compilation_only")
            .AddInterceptors(new StopBeforeConnectionInterceptor())
            .Options;
        await using var context = new ProjectDbContext(options);

        var exception = await Record.ExceptionAsync(() => query(new HistoryRepository(context)));

        Assert.True(exception is QueryCompiledException, $"{name}: {exception}");
    }

    private sealed class QueryCompiledException : Exception;

    private sealed class StopBeforeConnectionInterceptor : DbConnectionInterceptor
    {
        public override ValueTask<InterceptionResult> ConnectionOpeningAsync(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken = default)
        {
            throw new QueryCompiledException();
        }
    }
}

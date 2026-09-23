using System.Data.Common;
using ATMS.Data.Criteria;
using ATMS.Data.Enums;
using ATMS.Project.Data.Criteria.WorkTickets;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Project.Services.Tests.Repositories;

public class WorkTicketRepositoryTest
{
    [Fact]
    public async Task GetManyAsync_CompilesWithoutMultipleCollectionWarning()
    {
        var options = new DbContextOptionsBuilder<ProjectDbContext>()
            .UseNpgsql("Host=localhost;Database=query_compilation_only")
            .ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.MultipleCollectionIncludeWarning))
            .AddInterceptors(new StopBeforeConnectionInterceptor())
            .Options;
        await using var context = new ProjectDbContext(options);
        var repository = new WorkTicketRepository(context);

        // Compile the actual repository query with PostgreSQL, without connecting to any database.
        await Assert.ThrowsAsync<QueryCompiledException>(() => repository.GetManyAsync(
            new WorkTicketsByProjectCriteria(Guid.NewGuid(), null),
            new KeysetPaginationCriteria<WorkTicket>(null, 20, SortDirectionEnum.Asc),
            CancellationToken.None));
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

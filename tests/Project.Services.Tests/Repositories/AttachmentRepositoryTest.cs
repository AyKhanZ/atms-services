using System.Data.Common;
using ATMS.Project.Data.Criteria.Attachments;
using ATMS.Project.Data.Criteria.WorkTasks;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Project.Services.Tests.Repositories;

public class AttachmentRepositoryTest
{
    private static readonly AttachmentOwnerTasksFilter OwnerTasks = new() { ProjectId = Guid.NewGuid() };

    public static TheoryData<string, Func<AttachmentRepository, Task>> Queries => new()
    {
        {
            nameof(AttachmentRepository.GetManyAsync),
            repository => repository.GetManyAsync(
                OwnerTasks.And(new WorkTasksOfLiveWorkCriteria()),
                1001,
                CancellationToken.None)
        },
        {
            nameof(AttachmentRepository.GetAsync),
            repository => repository.GetAsync(Guid.NewGuid(), CancellationToken.None)
        },
        {
            nameof(AttachmentRepository.GetTicketCountsAsync),
            repository => repository.GetTicketCountsAsync(
                OwnerTasks.And(new WorkTasksOfLiveWorkCriteria()),
                CancellationToken.None)
        },
        {
            nameof(AttachmentRepository.FindAsync),
            repository => repository.FindAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None)
        },
        {
            nameof(AttachmentRepository.IsAttachmentExistAsync),
            repository => repository.IsAttachmentExistAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None)
        },
        {
            nameof(AttachmentRepository.CountByWorkTaskAsync),
            repository => repository.CountByWorkTaskAsync(Guid.NewGuid(), CancellationToken.None)
        }
    };

    // Compiles the real query for PostgreSQL without connecting: a projection EF cannot translate
    // fails here instead of on the first request.
    [Theory]
    [MemberData(nameof(Queries))]
    public async Task Query_TranslatesToPostgreSql(string name, Func<AttachmentRepository, Task> query)
    {
        var options = new DbContextOptionsBuilder<ProjectDbContext>()
            .UseNpgsql("Host=localhost;Database=query_compilation_only")
            .AddInterceptors(new StopBeforeConnectionInterceptor())
            .Options;
        await using var context = new ProjectDbContext(options);

        var exception = await Record.ExceptionAsync(() => query(new AttachmentRepository(context)));

        Assert.True(exception is QueryCompiledException, $"{name}: {exception}");
    }

    /* The author join reads past the soft-delete filter, and EF switches every filter off for the
       whole query when one is ignored: a deleted file came back in the list and 404-ed on open. */
    [Fact]
    public async Task GetManyAsync_LeavesOutDeletedFilesAndDeletedTasks()
    {
        var capture = new CaptureCommandInterceptor();
        var options = new DbContextOptionsBuilder<ProjectDbContext>()
            .UseNpgsql("Host=localhost;Database=query_capture_only")
            .AddInterceptors(new SuppressConnectionInterceptor(), capture)
            .Options;
        await using var context = new ProjectDbContext(options);

        await Assert.ThrowsAsync<QueryCompiledException>(() => new AttachmentRepository(context).GetManyAsync(
            OwnerTasks.And(new WorkTasksOfLiveWorkCriteria()),
            10,
            CancellationToken.None));

        var sql = capture.Sql!;
        Assert.Matches(@"NOT \(a\.""IsDeleted""\)", sql);
        Assert.Matches(@"NOT \(s\.""IsDeleted""\)", sql);
    }

    private sealed class QueryCompiledException : Exception;

    private sealed class SuppressConnectionInterceptor : DbConnectionInterceptor
    {
        public override ValueTask<InterceptionResult> ConnectionOpeningAsync(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(InterceptionResult.Suppress());
        }
    }

    private sealed class CaptureCommandInterceptor : DbCommandInterceptor
    {
        public string? Sql { get; private set; }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            Sql = command.CommandText;
            throw new QueryCompiledException();
        }
    }

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

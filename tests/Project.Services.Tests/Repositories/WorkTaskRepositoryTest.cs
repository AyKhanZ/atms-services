using ATMS.Data.Enums;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Project.Services.Tests.Realtime;

namespace Project.Services.Tests.Repositories;

public class WorkTaskRepositoryTest
{
    private const int InProgress = (int)WorkTaskStatusEnum.InProgress;

    [PostgresFact]
    public async Task GetRankBelowAsync_FindsTheNearestCardOfTheWholeColumnButNotTheCardItself()
    {
        await using var connection = await OpenTemporaryTasksAsync();
        await using var context = new ProjectDbContext(
            new DbContextOptionsBuilder<ProjectDbContext>().UseNpgsql(connection).Options);
        var moving = Guid.NewGuid();
        await ExecuteAsync(connection, $"""
            INSERT INTO "Tasks" ("Id", "StatusId", "Rank", "IsDeleted") VALUES
                (gen_random_uuid(), {InProgress}, 'm000000000zt', false),
                (gen_random_uuid(), {InProgress}, 'm000000000zsv', false),
                (gen_random_uuid(), {InProgress}, 'm000000000a', true),
                (gen_random_uuid(), {(int)WorkTaskStatusEnum.New}, 'm000000000b', false),
                ('{moving}', {InProgress}, 'm000000000c', false);
            """);
        var repository = new WorkTaskRepository(context);

        // Another project's card counts; a deleted one, another column and the card itself do not.
        Assert.Equal("m000000000zsv", await repository.GetRankBelowAsync(InProgress, null, moving, CancellationToken.None));
        Assert.Equal("m000000000zt", await repository.GetRankBelowAsync(InProgress, "m000000000zsv", moving, CancellationToken.None));
        Assert.Null(await repository.GetRankBelowAsync(InProgress, "m000000000zt", moving, CancellationToken.None));
        // Byte order, as the unique index compares: "zsv" comes before "zt".
        Assert.Equal("m000000000zsv", await repository.GetRankBelowAsync(InProgress, "m000000000z", moving, CancellationToken.None));
    }

    // A temporary table shadows the persistent one for this connection: only the columns the query
    // reads, with the collation the real column has.
    private static async Task<NpgsqlConnection> OpenTemporaryTasksAsync()
    {
        var connectionString = Environment.GetEnvironmentVariable("ATMS_REALTIME_TEST_DB");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ATMS_REALTIME_TEST_DB is required.");
        }

        var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await ExecuteAsync(connection, """
            CREATE TEMP TABLE "Tasks" (
                "Id" uuid NOT NULL,
                "StatusId" integer NOT NULL,
                "Rank" character varying(64) COLLATE "C" NOT NULL,
                "IsDeleted" boolean NOT NULL);
            """);
        return connection;
    }

    private static async Task ExecuteAsync(NpgsqlConnection connection, string sql)
    {
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }
}

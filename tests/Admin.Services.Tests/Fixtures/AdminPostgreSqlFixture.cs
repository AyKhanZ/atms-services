using ATMS.Admin.Data.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Testcontainers.PostgreSql;

namespace Admin.Services.Tests.Fixtures;

public sealed class AdminPostgreSqlFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer postgres = new PostgreSqlBuilder("postgres:16")
        .Build();

    public async Task InitializeAsync()
    {
        await postgres.StartAsync();
        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public Task DisposeAsync() => postgres.DisposeAsync().AsTask();

    public AdminDbContext CreateContext(params IInterceptor[] interceptors) => new(
        new DbContextOptionsBuilder<AdminDbContext>()
            .UseNpgsql(postgres.GetConnectionString())
            .AddInterceptors(interceptors)
            .Options);
}

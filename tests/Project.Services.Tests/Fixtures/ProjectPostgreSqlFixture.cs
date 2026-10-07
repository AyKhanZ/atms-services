using ATMS.Data.Enums;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Project.Services.Tests.Fixtures;

// A real PostgreSQL, because a row lock is what these tests are about and no fake has one.
public sealed class ProjectPostgreSqlFixture : IAsyncLifetime
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

    public ProjectDbContext CreateContext() => new(
        new DbContextOptionsBuilder<ProjectDbContext>()
            .UseNpgsql(postgres.GetConnectionString())
            .Options);

    public async Task<User> AddUserAsync()
    {
        var id = Guid.NewGuid();
        var user = new User
        {
            Id = id,
            Email = $"{id:N}@client.az",
            NormalizedEmail = $"{id:N}@CLIENT.AZ",
            Name = "Nigar",
            Surname = "Huseynova",
            AvatarPath = "avatar.png",
            UserType = (int)UserTypeEnum.Client,
            HasCompletedOnboarding = true
        };

        await using var context = CreateContext();
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    public async Task<WorkProject> AddProjectAsync()
    {
        var creator = await AddUserAsync();
        var project = new WorkProject
        {
            Id = Guid.NewGuid(),
            Code = Guid.NewGuid().ToString("N")[..10],
            Title = $"Customer portal {Guid.NewGuid():N}",
            ProjectTypeId = (int)ProjectTypeEnum.Standard,
            ProjectKindId = (int)ProjectKindEnum.External,
            ProjectStatusId = (int)ProjectStatusEnum.Draft,
            CreatedById = creator.Id,
            CreatedAt = DateTime.UtcNow
        };

        await using var context = CreateContext();
        context.WorkProjects.Add(project);
        await context.SaveChangesAsync();
        return project;
    }
}

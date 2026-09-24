using ATMS.Data.Enums;
using ATMS.Project.Data.Criteria.WorkTasks;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Project.Services.Tests.Criteria.WorkTasks;

public class WorkTaskBoardFilterTest
{
    private static readonly DateTime Today = new(2026, 9, 23, 0, 0, 0, DateTimeKind.Utc);

    private static readonly WorkTask[] Tasks =
    [
        Task("Open, overdue", WorkTaskStatusEnum.InProgress, Today.AddDays(-3)),
        Task("New, overdue", WorkTaskStatusEnum.New, Today.AddTicks(-1)),
        Task("Done, past deadline", WorkTaskStatusEnum.Done, Today.AddDays(-3)),
        Task("Due today", WorkTaskStatusEnum.New, Today),
        Task("Due later", WorkTaskStatusEnum.InProgress, Today.AddDays(5)),
        Task("No deadline", WorkTaskStatusEnum.New, null)
    ];

    [Fact]
    public void Apply_WithOverdueBefore_KeepsOnlyOpenWorkPastTheDeadline()
    {
        var result = new WorkTaskBoardFilter { OverdueBefore = Today }
            .Apply(Tasks.AsQueryable())
            .Select(task => task.Title)
            .ToArray();

        Assert.Equal(["Open, overdue", "New, overdue"], result);
    }

    [Fact]
    public void Apply_WithExcludeOverdueBefore_KeepsEverythingElse()
    {
        var result = new WorkTaskBoardFilter { ExcludeOverdueBefore = Today }
            .Apply(Tasks.AsQueryable())
            .Select(task => task.Title)
            .ToArray();

        Assert.Equal(["Done, past deadline", "Due today", "Due later", "No deadline"], result);
    }

    [Fact]
    public void Apply_OverdueAndTheRest_SplitTheListWithoutGapsOrOverlap()
    {
        var overdue = new WorkTaskBoardFilter { OverdueBefore = Today }.Apply(Tasks.AsQueryable()).ToArray();
        var rest = new WorkTaskBoardFilter { ExcludeOverdueBefore = Today }.Apply(Tasks.AsQueryable()).ToArray();

        Assert.Empty(overdue.Intersect(rest));
        Assert.Equal(Tasks.Length, overdue.Length + rest.Length);
    }

    [Fact]
    public void Apply_WithExcludeOverdueBefore_TranslatesToPostgreSql()
    {
        var options = new DbContextOptionsBuilder<ProjectDbContext>()
            .UseNpgsql("Host=localhost;Database=query-generation;Username=test")
            .Options;
        using var context = new ProjectDbContext(options);

        var sql = new WorkTaskBoardFilter { ExcludeOverdueBefore = Today }
            .Apply(context.WorkTasks)
            .ToQueryString();

        Assert.Contains("\"Deadline\" IS NULL", sql);
        Assert.Contains("\"Deadline\" >=", sql);
        Assert.Contains("\"StatusId\" = 3", sql);
    }

    private static WorkTask Task(string title, WorkTaskStatusEnum status, DateTime? deadline)
        => new() { Id = Guid.NewGuid(), Title = title, StatusId = (int)status, Deadline = deadline };
}

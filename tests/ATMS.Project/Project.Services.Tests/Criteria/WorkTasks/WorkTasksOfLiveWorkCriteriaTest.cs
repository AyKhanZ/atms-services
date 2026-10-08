using ATMS.Project.Data.Criteria.WorkTasks;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq.Expressions;

namespace Project.Services.Tests.Criteria.WorkTasks;

public class WorkTasksOfLiveWorkCriteriaTest
{
    [Theory]
    [InlineData(false, false, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    public void Apply_KeepsOnlyTasksWhoseProjectAndTicketAreNotDeleted(
        bool projectDeleted, bool ticketDeleted, bool kept)
    {
        var task = new WorkTask
        {
            Id = Guid.NewGuid(),
            WorkProject = new WorkProject { IsDeleted = projectDeleted },
            WorkTicket = new WorkTicket { IsDeleted = ticketDeleted }
        };

        var result = new WorkTasksOfLiveWorkCriteria().Apply(new[] { task }.AsQueryable()).ToArray();

        Assert.Equal(kept, result.Length == 1);
    }

    [Fact]
    public void Apply_TranslatesToPostgreSql()
    {
        var options = new DbContextOptionsBuilder<ProjectDbContext>()
            .UseNpgsql("Host=localhost;Database=query-generation;Username=test")
            .Options;
        using var context = new ProjectDbContext(options);

        var sql = new WorkTasksOfLiveWorkCriteria().Apply(context.WorkTasks).ToQueryString();

        Assert.Contains("\"Projects\"", sql);
        Assert.Contains("\"Tickets\"", sql);
        Assert.Contains("\"IsDeleted\"", sql);
    }

    [Fact]
    public void Apply_KeepsSubtasksAlongsideRootTasks()
    {
        var rootId = Guid.NewGuid();
        var project = new WorkProject { IsDeleted = false };
        var ticket = new WorkTicket { IsDeleted = false };
        var tasks = new[]
        {
            new WorkTask { Id = rootId, WorkProject = project, WorkTicket = ticket },
            new WorkTask
            {
                Id = Guid.NewGuid(), ParentWorkTaskId = rootId,
                WorkProject = project, WorkTicket = ticket
            }
        };

        var result = new WorkTasksOfLiveWorkCriteria().Apply(tasks.AsQueryable()).ToArray();

        Assert.Equal(2, result.Length);
    }

    [Fact]
    public void WorkTaskQueryFilter_ExcludesSoftDeletedTasks()
    {
        var options = new DbContextOptionsBuilder<ProjectDbContext>()
            .UseNpgsql("Host=localhost;Database=query-generation;Username=test")
            .Options;
        using var context = new ProjectDbContext(options);
        var entityType = Assert.IsAssignableFrom<IEntityType>(context.Model.FindEntityType(typeof(WorkTask)));
        var filter = Assert.IsAssignableFrom<Expression<Func<WorkTask, bool>>>(
            Assert.Single(entityType.GetDeclaredQueryFilters()).Expression);

        Assert.True(filter.Compile()(new WorkTask { IsDeleted = false }));
        Assert.False(filter.Compile()(new WorkTask { IsDeleted = true }));
    }
}

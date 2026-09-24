using ATMS.Data.Enums;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Services;
using Microsoft.EntityFrameworkCore;

namespace Project.Services.Tests.History;

public sealed class HistoryFieldMapTest
{
    /* A new column must be decided about: recorded in the history or left out on purpose. Without
       this test it would silently never show up there. */
    [Fact]
    public void EveryColumnOfARecordedEntity_IsEitherRecordedOrIgnored()
    {
        using var context = new ProjectDbContext(new DbContextOptionsBuilder<ProjectDbContext>()
            .UseNpgsql("Host=localhost;Database=model_only")
            .Options);
        var map = new HistoryFieldMap();

        var undecided = map.EntityTypes
            .SelectMany(type => context.Model.FindEntityType(type)!
                .GetProperties()
                .Where(property =>
                    !map.TryGetField(type, property.Name, out _) &&
                    !map.IsIgnored(type, property.Name))
                .Select(property => $"{type.Name}.{property.Name}"))
            .ToArray();

        Assert.Empty(undecided);
    }

    [Theory]
    [InlineData(typeof(WorkTask), nameof(WorkTask.StatusId), HistoryFieldEnum.Status)]
    [InlineData(typeof(WorkTicket), nameof(WorkTicket.WorkTicketStatusId), HistoryFieldEnum.Status)]
    [InlineData(typeof(WorkTicket), nameof(WorkTicket.WorkGroupId), HistoryFieldEnum.Milestone)]
    [InlineData(typeof(WorkProject), nameof(WorkProject.ProjectStatusId), HistoryFieldEnum.Status)]
    public void TryGetField_RecordedColumn_ReturnsItsField(Type entityType, string property, HistoryFieldEnum expected)
    {
        Assert.True(new HistoryFieldMap().TryGetField(entityType, property, out var field));
        Assert.Equal(expected, field);
    }

    [Theory]
    [InlineData(typeof(WorkTask), nameof(WorkTask.Rank))]
    [InlineData(typeof(WorkTask), nameof(WorkTask.DoneAt))]
    [InlineData(typeof(WorkTicket), nameof(WorkTicket.StatusId))]
    [InlineData(typeof(WorkProject), nameof(WorkProject.UpdatedAt))]
    public void TryGetField_NoiseColumn_IsNotRecorded(Type entityType, string property)
    {
        Assert.False(new HistoryFieldMap().TryGetField(entityType, property, out _));
    }
}

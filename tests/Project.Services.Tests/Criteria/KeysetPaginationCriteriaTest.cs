using ATMS.Data.Criteria;
using ATMS.Data.Enums;
using ATMS.Project.Data.Entities;

namespace Project.Services.Tests.Criteria;

public class KeysetPaginationCriteriaTest
{
    private static KeysetPaginationCriteria<WorkTask, DateTime?> ByDeadline(string cursor) =>
        new(cursor, 20, SortDirectionEnum.Asc, task => task.Deadline, task => task.Id, emptyKeysLast: true, order: "Deadline");

    [Fact]
    public void Constructor_WithAKeyThatIsNotThisOrdersType_RefusesTheCursorInsteadOfFailingLater()
    {
        var cursor = new KeysetCursor("not-a-date", Guid.NewGuid(), SortDirectionEnum.Asc, "Deadline").Encode();

        var exception = Assert.Throws<CriteriaException>(() => ByDeadline(cursor));

        Assert.Equal("cursor", exception.PropertyName);
    }

    [Fact]
    public void Constructor_WithACursorOfAnotherOrder_RefusesIt()
    {
        var cursor = KeysetCursor.For(DateTime.UtcNow, Guid.NewGuid(), SortDirectionEnum.Asc, "DoneAt").Encode();

        Assert.Throws<CriteriaException>(() => ByDeadline(cursor));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Constructor_WithThisOrdersOwnCursor_ContinuesFromIt(bool emptyKey)
    {
        var id = Guid.NewGuid();
        var cursor = KeysetCursor.For<DateTime?>(emptyKey ? null : DateTime.UtcNow, id, SortDirectionEnum.Asc, "Deadline")
            .Encode();

        var criteria = ByDeadline(cursor);

        Assert.Equal(id, criteria.Cursor!.Id);
    }
}

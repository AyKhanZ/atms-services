using ATMS.Project.Services.Board;

namespace Project.Services.Tests.Board;

public class WorkTaskBoardPositionServiceTest
{
    private static readonly WorkTaskBoardPositionService Service = new();

    [Theory]
    [InlineData(null, null)]
    [InlineData(null, "m00000001v")]
    [InlineData("m00000001v", null)]
    [InlineData("m00000001v", "m00000002v")]
    [InlineData("a", "b")]
    [InlineData("a", "a1")]
    [InlineData("az", "b")]
    [InlineData("0i", "1")]
    public void Between_ReturnsKeyThatSortsStrictlyBetweenItsNeighbours(string? before, string? after)
    {
        var key = Service.Between(before, after);

        if (before is not null) Assert.True(string.CompareOrdinal(before, key) < 0, $"{before} < {key}");
        if (after is not null) Assert.True(string.CompareOrdinal(key, after) < 0, $"{key} < {after}");
        Assert.False(key.EndsWith('0'));
    }

    // Dropping card after card into the same gap is the worst case: keys must keep fitting and
    // stay short enough for the column.
    [Fact]
    public void Between_KeepsFittingWhenCardsLandInTheSameGapAgainAndAgain()
    {
        var above = "m00000001v";
        var below = "m00000002v";
        for (var i = 0; i < 200; i++)
        {
            var key = Service.Between(above, below);
            Assert.True(string.CompareOrdinal(above, key) < 0 && string.CompareOrdinal(key, below) < 0);
            below = key;
        }

        Assert.True(below.Length <= Service.MaxLength);
    }

    [Fact]
    public void Between_KeepsAddingOnTopOfTheBoard()
    {
        var top = "m00000001v";
        for (var i = 0; i < 200; i++)
        {
            var key = Service.Between(null, top);
            Assert.True(string.CompareOrdinal(key, top) < 0);
            top = key;
        }
    }

    [Fact]
    public void Between_RejectsNeighboursInTheWrongOrder()
    {
        Assert.Throws<ArgumentException>(() => Service.Between("b", "a"));
    }

    [Fact]
    public void Between_AddingOneHundredThousandTasksAtTheTopStaysWithinTheColumnLimit()
    {
        string? top = null;
        for (var index = 0; index < 100_000; index++)
        {
            var next = Service.Between(null, top);
            Assert.InRange(next.Length, 1, Service.MaxLength);
            Assert.True(top is null || string.CompareOrdinal(next, top) < 0);
            top = next;
        }
    }

    [Fact]
    public void Between_ExhaustedIntervalFailsBeforeWritingAnOversizedKey()
    {
        var above = "a";
        var below = "a" + new string('0', 62) + "1";
        Assert.Throws<InvalidOperationException>(() => Service.Between(above, below));
    }
}

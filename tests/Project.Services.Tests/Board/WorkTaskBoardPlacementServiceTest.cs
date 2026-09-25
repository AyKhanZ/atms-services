using ATMS.Application.Exceptions.Conflict;
using ATMS.Data.Criteria.Interfaces;
using ATMS.Data.Enums;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Models.WorkTasks;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Board;
using Moq;

namespace Project.Services.Tests.Board;

public class WorkTaskBoardPlacementServiceTest
{
    private readonly Mock<IWorkTaskRepository> _repository = new();
    private readonly WorkTaskBoardPositionService _positions = new();

    private WorkTaskBoardPlacementService Service() => new(_repository.Object, _positions);

    private static WorkTask Card(string rank = "m5") =>
        new() { Id = Guid.NewGuid(), StatusId = (int)WorkTaskStatusEnum.New, Rank = rank };

    [Fact]
    public async Task SaveAsync_WhenAnotherCardTookThePlaceMeanwhile_GoesRightBelowIt()
    {
        // Two drops between m1 and m3 both computed m2; the other one was saved first.
        var card = Card("m2");
        _repository.SetupSequence(repository => repository.TrySaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false)
            .ReturnsAsync(true);
        _repository.Setup(repository => repository.GetNextRankAsync(card.StatusId, "m2", It.IsAny<CancellationToken>()))
            .ReturnsAsync("m3");

        await Service().SaveAsync(card, CancellationToken.None);

        Assert.True(string.CompareOrdinal("m2", card.Rank) < 0);
        Assert.True(string.CompareOrdinal(card.Rank, "m3") < 0);
        _repository.Verify(repository => repository.TrySaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task SaveAsync_WhenThePlaceKeepsBeingTaken_GivesUpWithAConflict()
    {
        var card = Card("m2");
        _repository.Setup(repository => repository.TrySaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<ConflictException>(() => Service().SaveAsync(card, CancellationToken.None));

        _repository.Verify(repository => repository.TrySaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact]
    public async Task PlaceBetweenAsync_WhenTheGapIsUsedUp_SpreadsTheColumnOnceAndPlacesInTheNewGap()
    {
        var card = Card();
        var above = Guid.NewGuid();
        var below = Guid.NewGuid();
        _repository.SetupSequence(repository => repository.GetRanksAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<ICriteria<WorkTask>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, string> { [above] = "a", [below] = "a" + new string('0', 62) + "1" })
            .ReturnsAsync(new Dictionary<Guid, string> { [above] = "m000000001000v", [below] = "m000000002000v" });

        await Service().PlaceBetweenAsync(card, above, below, Mock.Of<ICriteria<WorkTask>>(), CancellationToken.None);

        _repository.Verify(repository => repository.RenumberColumnAsync(card.StatusId, It.IsAny<CancellationToken>()), Times.Once);
        Assert.True(string.CompareOrdinal("m000000001000v", card.Rank) < 0);
        Assert.True(string.CompareOrdinal(card.Rank, "m000000002000v") < 0);
    }

    [Fact]
    public async Task PlaceOnTopAsync_InAnEmptyColumn_GivesTheCardAPlace()
    {
        var card = new WorkTask { Id = Guid.NewGuid(), StatusId = (int)WorkTaskStatusEnum.New };
        _repository.Setup(repository => repository.GetTopPlaceAsync(card.StatusId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkTaskBoardPlace?)null);

        await Service().PlaceOnTopAsync(card, CancellationToken.None);

        Assert.False(string.IsNullOrEmpty(card.Rank));
    }

    [Fact]
    public async Task PlaceOnTopAsync_GoesAboveTheCurrentFirstCard()
    {
        var card = Card("m9");
        _repository.Setup(repository => repository.GetTopPlaceAsync(card.StatusId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WorkTaskBoardPlace(Guid.NewGuid(), "m1"));

        await Service().PlaceOnTopAsync(card, CancellationToken.None);

        Assert.True(string.CompareOrdinal(card.Rank, "m1") < 0);
    }

    [Fact]
    public async Task PlaceOnTopAsync_WhenAnotherFirstCardHasTheSameKey_GoesAboveIt()
    {
        // A card back from Done keeps its old key, and a card placed on top since then can hold it.
        var card = Card("m5");
        _repository.Setup(repository => repository.GetTopPlaceAsync(card.StatusId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WorkTaskBoardPlace(Guid.NewGuid(), "m5"));

        await Service().PlaceOnTopAsync(card, CancellationToken.None);

        Assert.True(string.CompareOrdinal(card.Rank, "m5") < 0);
    }

    [Fact]
    public async Task PlaceOnTopAsync_WhenTheCardIsAlreadyFirst_KeepsItsKey()
    {
        var card = Card("m5");
        _repository.Setup(repository => repository.GetTopPlaceAsync(card.StatusId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WorkTaskBoardPlace(card.Id, "m5"));

        await Service().PlaceOnTopAsync(card, CancellationToken.None);

        Assert.Equal("m5", card.Rank);
    }
}

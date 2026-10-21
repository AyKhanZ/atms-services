using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Domain.Board;
using Moq;

namespace Project.Services.Tests.Domain.Board;

internal static class TestBoardColumns
{
    public static WorkTaskColumnService Service()
    {
        var repository = new Mock<IBoardColumnRepository>();
        repository
            .Setup(repo => repo.FindAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int id, CancellationToken _) => Column(id, id));
        return new WorkTaskColumnService(repository.Object);
    }

    public static BoardColumn Column(int id, int category) =>
        new()
        {
            Id = id,
            Category = category,
            Code = id.ToString()
        };
}

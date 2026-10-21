using ATMS.Project.Contracts.Models.BoardColumns;
using ATMS.Project.Contracts.Requests.BoardColumns;
using ATMS.Project.Services.Domain.Board.Interfaces;
using ATMS.Project.Services.Handlers.BoardColumns;
using Moq;

namespace Project.Services.Tests.Handlers.BoardColumns;

public sealed class GetBoardColumnsHandlerTest
{
    private readonly Mock<IBoardColumnCacheService> _cache = new();

    [Fact]
    public async Task Handle_ReturnsTheCachedList()
    {
        var columns = new[]
        {
            new BoardColumnModel
            {
                Id = 1,
                Code = "New",
                Name = "New",
                Category = 1,
                Order = 1
            }
        };
        _cache
            .Setup(cache => cache.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(columns);

        var result = await new GetBoardColumnsHandler(_cache.Object)
            .Handle(new GetBoardColumnsRequest(), CancellationToken.None);

        Assert.Same(columns, result);
    }
}

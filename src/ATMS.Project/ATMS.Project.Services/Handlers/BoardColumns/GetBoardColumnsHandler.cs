using ATMS.Project.Contracts.Models.BoardColumns;
using ATMS.Project.Contracts.Requests.BoardColumns;
using ATMS.Project.Services.Domain.Board.Interfaces;
using MediatR;

namespace ATMS.Project.Services.Handlers.BoardColumns;

public sealed class GetBoardColumnsHandler(IBoardColumnCacheService cache)
    : IRequestHandler<GetBoardColumnsRequest, BoardColumnModel[]>
{
    public Task<BoardColumnModel[]> Handle(GetBoardColumnsRequest request, CancellationToken cancellationToken)
    {
        return cache.GetAllAsync(cancellationToken);
    }
}

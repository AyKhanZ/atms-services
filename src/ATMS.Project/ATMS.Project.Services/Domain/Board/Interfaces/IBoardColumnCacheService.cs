using ATMS.Project.Contracts.Models.BoardColumns;

namespace ATMS.Project.Services.Domain.Board.Interfaces;

public interface IBoardColumnCacheService
{
    Task<BoardColumnModel[]> GetAllAsync(CancellationToken cancellationToken);
}

using ATMS.Project.Data.Entities;

namespace ATMS.Project.Data.Repositories.Interfaces;

public interface IBoardColumnRepository
{
    Task<BoardColumn[]> GetOrderedAsync(CancellationToken cancellationToken);

    Task<BoardColumn?> FindAsync(int id, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken);
}

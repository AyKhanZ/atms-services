using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ATMS.Project.Data.Repositories;

public sealed class BoardColumnRepository(ProjectDbContext context) : IBoardColumnRepository
{
    public Task<BoardColumn[]> GetOrderedAsync(CancellationToken cancellationToken)
    {
        return context.BoardColumns
            .AsNoTracking()
            .Include(column => column.Translations)
            .OrderBy(column => column.Order)
            .ToArrayAsync(cancellationToken);
    }

    public Task<BoardColumn?> FindAsync(int id, CancellationToken cancellationToken)
    {
        return context.BoardColumns
            .FirstOrDefaultAsync(column => column.Id == id, cancellationToken);
    }

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken)
    {
        return context.BoardColumns.AnyAsync(column => column.Id == id, cancellationToken);
    }
}

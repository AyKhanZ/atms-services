using System.Linq.Expressions;
using ATMS.Data.Criteria;
using ATMS.Project.Data.Entities;

namespace ATMS.Project.Data.Repositories.Interfaces;

public interface IWorkProjectRepository
{
    Task<WorkProject?> GetAsync(
        Guid id,
        ACriteria<WorkProject> accessCriteria,
        CancellationToken cancellationToken);

    Task<PagedResult<WorkProject>> GetAsync(
        ACriteria<WorkProject> filterCriteria,
        PaginationCriteria<WorkProject> pagination,
        CancellationToken cancellationToken);

    Task<WorkProject?> FindAsync(Guid id, CancellationToken cancellationToken);

    Task CreateAsync(WorkProject entity, CancellationToken cancellationToken);

    Task<bool> IsExistAsync(Expression<Func<WorkProject, bool>> predicate, CancellationToken cancellationToken);

    Task SaveAsync(CancellationToken cancellationToken);
}

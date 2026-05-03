using System.Linq.Expressions;
using ATMS.Data.Criterias;
using ATMS.Project.Data.Entities;

namespace ATMS.Project.Data.Repositories.Interfaces;

public interface IOrganizationRepository
{
    Task<Organization?> GetAsync(Expression<Func<Organization, bool>> predicate, CancellationToken cancellationToken);

    Task<PagedResult<Organization>> GetAsync(ACriteria<Organization> filterCriteria,
        PaginationCriteria<Organization> pagination, CancellationToken cancellationToken);
    
    Task<Organization?> FindAsync(Expression<Func<Organization, bool>> predicate, CancellationToken cancellationToken);
    
    Task CreateAsync(Organization entity, CancellationToken cancellationToken);
    
    Task<bool> IsExistAsync(Expression<Func<Organization, bool>> predicate, CancellationToken cancellationToken);
    
    Task SaveAsync(CancellationToken cancellationToken);
}
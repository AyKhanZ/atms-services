using System.Linq.Expressions;
using ATMS.Admin.Data.Entities.Dictionaries;

namespace ATMS.Admin.Data.Repositories.Interfaces;

public interface IDictionariesRepository
{
    Task<List<Gender>> GetGendersAsync(CancellationToken cancellationToken = default);
    
    Task<List<UserStatus>> GetUserStatusesAsync(CancellationToken cancellationToken = default);
    
    Task<List<MaritalStatus>> GetMaritalStatusesAsync(CancellationToken cancellationToken = default);
    
    Task<bool> IsUserStatusExistAsync(Expression<Func<UserStatus, bool>> predicate,
        CancellationToken cancellationToken = default);
    
    Task<bool> IsMaritalStatusExistAsync(Expression<Func<MaritalStatus, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task<bool> IsGenderExistAsync(Expression<Func<Gender, bool>> predicate,
        CancellationToken cancellationToken = default);
}

using ATMS.Admin.Data.Entities.Dictionaries;

namespace ATMS.Admin.Data.Repositories.Interfaces;

public interface IPermissionRepository
{
    Task<List<Permission>> GetAsync(CancellationToken cancellationToken);
    
    Task<List<int>> GetIdsAsync(CancellationToken cancellationToken);
    
    Task<List<int>> GetExistingIdsAsync(int[] ids, CancellationToken cancellationToken);
}

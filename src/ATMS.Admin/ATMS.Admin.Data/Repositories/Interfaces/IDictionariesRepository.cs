using ATMS.Admin.Data.Entities.Dictionaries;

namespace ATMS.Admin.Data.Repositories.Interfaces;

public interface IDictionariesRepository
{
    Task<List<UserType>> GetUserTypesAsync(CancellationToken cancellationToken = default);
    
    Task<List<Gender>> GetGendersAsync(CancellationToken cancellationToken = default);
    
    Task<List<UserStatus>> GetUserStatusesAsync(CancellationToken cancellationToken = default);
    
    Task<List<MaritalStatus>> GetMaritalStatusesAsync(CancellationToken cancellationToken = default);
}

using ATMS.Admin.Data.Entities;

namespace ATMS.Admin.Data.Interfaces;

public interface IUserRepository
{
    Task CreateAsync(User user, CancellationToken cancellationToken);

    Task<User?> FindByEmail(string email, CancellationToken cancellationToken);
    Task<List<User>> GetAsync();

    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<List<Role>> GetRolesAsync(Guid userId, CancellationToken cancellationToken);

    Task<bool> IsExistAsync(string email, CancellationToken cancellationToken);

    Task SaveAsync(CancellationToken cancellationToken);
}

using ATMS.Admin.Data.Entities;

namespace ATMS.Admin.Data.Interfaces;

public interface IUserRepository
{
    Task CreateAsync(User user, CancellationToken cancellationToken);

    Task<List<User>> GetAsync();

    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> IsExistAsync(string email, CancellationToken cancellationToken);
}

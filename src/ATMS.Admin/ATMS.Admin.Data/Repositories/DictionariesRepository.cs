using ATMS.Admin.Data.DbContexts;
using ATMS.Admin.Data.Entities.Dictionaries;
using ATMS.Admin.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ATMS.Admin.Data.Repositories;

public class DictionariesRepository(AdminDbContext context) : IDictionariesRepository
{
    public Task<List<UserType>> GetUserTypesAsync(CancellationToken cancellationToken = default)
    {
        return context.UserTypes
            .Include(p => p.Translations)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<List<Gender>> GetGendersAsync(CancellationToken cancellationToken = default)
    {
        return context.Genders
            .Include(p => p.Translations)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<List<UserStatus>> GetUserStatusesAsync(CancellationToken cancellationToken = default)
    {
        return context.UserStatuses
            .Include(p => p.Translations)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<List<MaritalStatus>> GetMaritalStatusesAsync(CancellationToken cancellationToken = default)
    {
        return context.MaritalStatuses
            .Include(p => p.Translations)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}

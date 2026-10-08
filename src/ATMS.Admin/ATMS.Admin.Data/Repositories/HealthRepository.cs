using ATMS.Admin.Data.DbContexts;
using ATMS.Admin.Data.Repositories.Interfaces;

namespace ATMS.Admin.Data.Repositories;

public sealed class HealthRepository(AdminDbContext context) : IHealthRepository
{
    public Task<bool> IsReadyAsync(CancellationToken cancellationToken)
    {
        return context.Database.CanConnectAsync(cancellationToken);
    }
}

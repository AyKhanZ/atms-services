using ATMS.Admin.Data.DbContexts;
using ATMS.Admin.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ATMS.Admin.Data.Repositories;

public class HealthRepository(AdminDbContext context) : IHealthRepository
{
    public Task<bool> IsReadyAsync(CancellationToken cancellationToken)
    {
        return context.Database.CanConnectAsync(cancellationToken);
    }
}

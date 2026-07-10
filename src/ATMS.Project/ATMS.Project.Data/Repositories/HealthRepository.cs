using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ATMS.Project.Data.Repositories;

public class HealthRepository(ProjectDbContext context) : IHealthRepository
{
    public Task<bool> IsReadyAsync(CancellationToken cancellationToken)
    {
        return context.Database.CanConnectAsync(cancellationToken);
    }
}

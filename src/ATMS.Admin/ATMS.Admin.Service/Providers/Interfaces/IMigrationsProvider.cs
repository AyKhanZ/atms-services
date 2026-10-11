using ATMS.Admin.Contracts.Models;

namespace ATMS.Admin.Service.Providers.Interfaces;

public interface IMigrationsProvider
{
    Task<MigrationModel> ApplyAsync(CancellationToken cancellationToken);
}

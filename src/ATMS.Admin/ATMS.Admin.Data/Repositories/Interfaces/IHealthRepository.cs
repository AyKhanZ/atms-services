namespace ATMS.Admin.Data.Repositories.Interfaces;

public interface IHealthRepository
{
    Task<bool> IsReadyAsync(CancellationToken cancellationToken);
}

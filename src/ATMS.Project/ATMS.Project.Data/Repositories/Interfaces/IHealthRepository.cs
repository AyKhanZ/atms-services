namespace ATMS.Project.Data.Repositories.Interfaces;

public interface IHealthRepository
{
    Task<bool> IsReadyAsync(CancellationToken cancellationToken);
}

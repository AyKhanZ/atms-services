namespace ATMS.Admin.Service.Infrastructure.Interfaces;

public interface IDataInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}

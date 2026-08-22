namespace ATMS.Project.Data.Services.Interfaces;

public interface IEntityCodeGenerator
{
    Task<string> GetNextAsync(CancellationToken cancellationToken);
}

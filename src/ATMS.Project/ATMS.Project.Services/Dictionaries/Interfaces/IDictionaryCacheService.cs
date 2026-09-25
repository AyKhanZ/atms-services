using ATMS.Application.Models;

namespace ATMS.Project.Services.Dictionaries.Interfaces;

public interface IDictionaryCacheService
{
    Task<DictionaryModel[]> GetProjectKindsAsync(CancellationToken cancellationToken);

    Task<DictionaryModel[]> GetProjectStatusesAsync(CancellationToken cancellationToken);

    Task<DictionaryModel[]> GetProjectTypesAsync(CancellationToken cancellationToken);

    Task<DictionaryModel[]> GetWorkGroupStatusesAsync(CancellationToken cancellationToken);

    Task<DictionaryModel[]> GetWorkItemPrioritiesAsync(CancellationToken cancellationToken);

    Task<DictionaryModel[]> GetWorkTaskStatusesAsync(CancellationToken cancellationToken);

    Task<DictionaryModel[]> GetWorkTicketStatusesAsync(CancellationToken cancellationToken);

    Task<DictionaryModel[]> GetWorkTicketTypesAsync(CancellationToken cancellationToken);
}

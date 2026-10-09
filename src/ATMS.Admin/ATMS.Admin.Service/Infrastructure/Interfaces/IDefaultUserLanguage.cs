namespace ATMS.Admin.Service.Infrastructure.Interfaces;

public interface IDefaultUserLanguage
{
    Task<int> GetLanguageIdAsync(CancellationToken cancellationToken = default);
}

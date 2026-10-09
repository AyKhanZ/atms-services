using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Application.Exceptions.Configuration;
using ATMS.Application.Exceptions.Enums;
using ATMS.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace ATMS.Admin.Service.Infrastructure;

public interface IDefaultUserLanguage
{
    Task<int> GetLanguageIdAsync(CancellationToken cancellationToken = default);
}

public sealed class DefaultUserLanguage(
    IOptions<LocalizationOptions> localizationOptions,
    IDictionariesRepository dictionariesRepository) : IDefaultUserLanguage
{
    public async Task<int> GetLanguageIdAsync(CancellationToken cancellationToken = default)
    {
        var code = localizationOptions.Value.DefaultLanguage;
        var languages = await dictionariesRepository.GetLanguagesAsync(cancellationToken);
        var language = languages.FirstOrDefault(item =>
            string.Equals(item.Code, code, StringComparison.OrdinalIgnoreCase));

        if (language is null)
        {
            throw new ConfigurationException(
                ConfigurationErrorTypeEnum.MissingSeedData,
                $"Language '{code}' was not found in the Languages dictionary.");
        }

        return language.Id;
    }
}

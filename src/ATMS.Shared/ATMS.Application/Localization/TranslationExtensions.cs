using ATMS.Application.Models;
using ATMS.Data;
using ATMS.Data.Interfaces;

namespace ATMS.Application.Localization;

public static class TranslationExtensions
{
    public static string Resolve(
        this IEnumerable<ITranslation> translations,
        string language,
        string fallback)
    {
        var list = translations.ToArray();
        return list
                   .FirstOrDefault(t => t.Language == language)?.Name
               ?? list
                   .FirstOrDefault(t => t.Language == SupportedLanguages.English)?.Name
               ?? fallback;
    }
    
    public static DictionaryModel ToDictionaryModel(
        this TranslatableDictionaryEntity entity,
        IEnumerable<ITranslation> translations,
        string language)
    {
        return new DictionaryModel
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = translations.Resolve(language, entity.Code)
        };
    }
}

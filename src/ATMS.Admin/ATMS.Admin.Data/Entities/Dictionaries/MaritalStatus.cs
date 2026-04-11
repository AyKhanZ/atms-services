using ATMS.Data;

namespace ATMS.Admin.Data.Entities.Dictionaries;

public class MaritalStatus : TranslatableDictionaryEntity
{
    public ICollection<MaritalStatusTranslation> Translations { get; set; } = [];
}

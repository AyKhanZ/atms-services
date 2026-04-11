using ATMS.Data;

namespace ATMS.Admin.Data.Entities.Dictionaries;

public class Gender : TranslatableDictionaryEntity
{
    public ICollection<GenderTranslation> Translations { get; set; } = [];
}

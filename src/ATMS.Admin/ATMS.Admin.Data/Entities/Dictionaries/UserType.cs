using ATMS.Data;

namespace ATMS.Admin.Data.Entities.Dictionaries;

public class UserType : TranslatableDictionaryEntity
{
    public ICollection<UserTypeTranslation> Translations { get; set; } = [];
}

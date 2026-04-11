using ATMS.Data;

namespace ATMS.Admin.Data.Entities.Dictionaries;

public class UserStatus : TranslatableDictionaryEntity
{
    public ICollection<UserStatusTranslation> Translations { get; set; } = [];
}

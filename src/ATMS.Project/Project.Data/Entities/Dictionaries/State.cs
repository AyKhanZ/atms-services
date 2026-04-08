using ATMS.Data;

namespace Project.Data.Entities.Dictionaries;

public class State : TranslatableDictionaryEntity
{
    public ICollection<StateTranslation> Translations { get; set; } = [];
}

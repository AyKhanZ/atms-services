using ATMS.Data;

namespace Project.Data.Entities.Dictionaries;

public class WorkTaskStatus : TranslatableDictionaryEntity
{
    public ICollection<WorkTaskStatusTranslation> Translations { get; set; } = [];
}

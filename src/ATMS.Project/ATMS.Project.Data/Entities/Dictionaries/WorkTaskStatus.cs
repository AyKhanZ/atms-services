using ATMS.Data;

namespace ATMS.Project.Data.Entities.Dictionaries;

public class WorkTaskStatus : TranslatableDictionaryEntity
{
    public ICollection<WorkTaskStatusTranslation> Translations { get; set; } = [];
}

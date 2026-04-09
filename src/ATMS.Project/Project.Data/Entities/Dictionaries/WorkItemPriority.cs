using ATMS.Data;

namespace Project.Data.Entities.Dictionaries;

public class WorkItemPriority : TranslatableDictionaryEntity
{
    public ICollection<WorkItemPriorityTranslation> Translations { get; set; } = [];
}

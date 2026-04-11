using ATMS.Data;

namespace ATMS.Project.Data.Entities.Dictionaries;

public class WorkItemPriority : TranslatableDictionaryEntity
{
    public ICollection<WorkItemPriorityTranslation> Translations { get; set; } = [];
}

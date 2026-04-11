using ATMS.Data;

namespace ATMS.Project.Data.Entities.Dictionaries;

public class WorkGroupStatus : TranslatableDictionaryEntity
{
    public ICollection<WorkGroupStatusTranslation> Translations { get; set; } = [];
}

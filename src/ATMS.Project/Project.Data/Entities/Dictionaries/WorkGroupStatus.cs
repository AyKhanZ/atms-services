using ATMS.Data;

namespace Project.Data.Entities.Dictionaries;

public class WorkGroupStatus : TranslatableDictionaryEntity
{
    public ICollection<WorkGroupStatusTranslation> Translations { get; set; } = [];
}

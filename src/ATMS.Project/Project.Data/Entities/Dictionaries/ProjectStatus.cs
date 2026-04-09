using ATMS.Data;

namespace Project.Data.Entities.Dictionaries;

public class ProjectStatus : TranslatableDictionaryEntity
{
    public ICollection<ProjectStatusTranslation> Translations { get; set; } = [];
}

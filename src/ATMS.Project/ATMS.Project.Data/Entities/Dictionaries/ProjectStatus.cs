using ATMS.Data;

namespace ATMS.Project.Data.Entities.Dictionaries;

public class ProjectStatus : TranslatableDictionaryEntity
{
    public ICollection<ProjectStatusTranslation> Translations { get; set; } = [];
}

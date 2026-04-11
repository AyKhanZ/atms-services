using ATMS.Data;

namespace ATMS.Project.Data.Entities.Dictionaries;

public class ProjectKind : TranslatableDictionaryEntity
{
    public ICollection<ProjectKindTranslation> Translations { get; set; } = [];
}

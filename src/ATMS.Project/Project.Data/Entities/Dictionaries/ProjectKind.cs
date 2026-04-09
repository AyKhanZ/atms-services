using ATMS.Data;

namespace Project.Data.Entities.Dictionaries;

public class ProjectKind : TranslatableDictionaryEntity
{
    public ICollection<ProjectKindTranslation> Translations { get; set; } = [];
}

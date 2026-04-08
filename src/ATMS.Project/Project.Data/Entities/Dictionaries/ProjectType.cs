using ATMS.Data;

namespace Project.Data.Entities.Dictionaries;

public class ProjectType : TranslatableDictionaryEntity
{
    public ICollection<ProjectTypeTranslation> Translations { get; set; } = [];
}

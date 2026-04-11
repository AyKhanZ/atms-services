using ATMS.Data;

namespace ATMS.Project.Data.Entities.Dictionaries;

public class ProjectType : TranslatableDictionaryEntity
{
    public ICollection<ProjectTypeTranslation> Translations { get; set; } = [];
}

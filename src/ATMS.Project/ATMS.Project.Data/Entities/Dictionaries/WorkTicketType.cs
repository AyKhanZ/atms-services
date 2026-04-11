using ATMS.Data;

namespace ATMS.Project.Data.Entities.Dictionaries;

public class WorkTicketType : TranslatableDictionaryEntity
{
    public ICollection<WorkTicketTypeTranslation> Translations { get; set; } = [];
}

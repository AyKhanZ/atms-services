using ATMS.Data;

namespace ATMS.Project.Data.Entities.Dictionaries;

public class WorkTicketStatus : TranslatableDictionaryEntity
{
    public ICollection<WorkTicketStatusTranslation> Translations { get; set; } = [];
}

using ATMS.Data;

namespace Project.Data.Entities.Dictionaries;

public class WorkTicketType : TranslatableDictionaryEntity
{
    public ICollection<WorkTicketTypeTranslation> Translations { get; set; } = [];
}

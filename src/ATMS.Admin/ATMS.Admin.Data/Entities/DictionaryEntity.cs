namespace ATMS.Admin.Data.Entities;

public class DictionaryEntity<TKey> : BaseEntity<TKey>
{
    public string Name { get; set; }
    public string Code { get; set; }
}

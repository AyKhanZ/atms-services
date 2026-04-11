namespace ATMS.Data;

public class DictionaryEntity<TKey> : BaseEntity<TKey>
{
    public string Name { get; set; }
    public string Code { get; set; }
}
public class DictionaryEntity : DictionaryEntity<int>;

namespace ATMS.Data;

public abstract class TranslatableDictionaryEntity<TKey> : BaseEntity<TKey>
{
    public string Code { get; set; } = null!;
}
public abstract class TranslatableDictionaryEntity : TranslatableDictionaryEntity<int>;

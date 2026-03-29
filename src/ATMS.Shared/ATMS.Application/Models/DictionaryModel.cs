namespace ATMS.Application.Models;

public class DictionaryModel<TKey>
{
    public TKey Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
}

public class DictionaryModel : DictionaryModel<int>;

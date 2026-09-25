using ATMS.Application.Models;

namespace ATMS.Project.Contracts.Models.History;

public class HistoryValueModel : DictionaryModel<string>
{
    public HistoryPersonModel? Person { get; set; }
}

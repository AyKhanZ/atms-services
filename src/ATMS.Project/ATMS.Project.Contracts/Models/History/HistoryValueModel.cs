using ATMS.Application.Models;
using ATMS.Project.Contracts.Models.Users;

namespace ATMS.Project.Contracts.Models.History;

public sealed class HistoryValueModel : DictionaryModel<string>
{
    public PersonModel? Person { get; set; }
}

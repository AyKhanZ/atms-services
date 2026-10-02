using ATMS.Project.Contracts.Models.Users;

namespace ATMS.Project.Contracts.Models.History;

public class HistoryChangeModel
{
    public int Field { get; set; }

    public PersonModel? Person { get; set; }

    public HistoryValueModel? OldValue { get; set; }

    public HistoryValueModel? NewValue { get; set; }
}

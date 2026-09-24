namespace ATMS.Project.Contracts.Models.History;

public class HistoryChangeModel
{
    public int Field { get; set; }

    public HistoryPersonModel? Person { get; set; }

    public HistoryValueModel? OldValue { get; set; }

    public HistoryValueModel? NewValue { get; set; }
}

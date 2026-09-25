namespace ATMS.Project.Contracts.Models.History;

public class HistoryStateModel
{
    public HistoryValueModel Status { get; set; }

    public DateTime? ChangedAt { get; set; }

    public HistoryPersonModel? ChangedBy { get; set; }
}

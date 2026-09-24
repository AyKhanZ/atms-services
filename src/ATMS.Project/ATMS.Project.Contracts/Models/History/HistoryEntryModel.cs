namespace ATMS.Project.Contracts.Models.History;

public class HistoryEntryModel
{
    public Guid Id { get; set; }

    public int EntityType { get; set; }

    public int Action { get; set; }

    public DateTime CreatedAt { get; set; }

    public HistoryPersonModel? CreatedBy { get; set; }

    public HistoryValueModel? Subject { get; set; }

    public IReadOnlyCollection<HistoryChangeModel> Changes { get; set; } = [];
}

using ATMS.Project.Contracts.Models.Users;

namespace ATMS.Project.Contracts.Models.History;

public sealed class HistoryEntryModel
{
    public Guid Id { get; set; }

    public int EntityType { get; set; }

    public int Action { get; set; }

    public DateTime CreatedAt { get; set; }

    public PersonModel? CreatedBy { get; set; }

    public HistoryValueModel? Subject { get; set; }

    public IReadOnlyCollection<HistoryChangeModel> Changes { get; set; } = [];
}

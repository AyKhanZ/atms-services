using ATMS.Project.Contracts.Models.Users;

namespace ATMS.Project.Contracts.Models.History;

public sealed class HistoryStateModel
{
    public HistoryValueModel Status { get; set; }

    public DateTime? ChangedAt { get; set; }

    public PersonModel? ChangedBy { get; set; }
}

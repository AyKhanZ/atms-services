using ATMS.Application.Models;

namespace ATMS.Project.Data.Models.Dashboard;

public sealed class DashboardEntityRow : DictionaryModel<Guid>
{
    public int Count { get; init; }
}

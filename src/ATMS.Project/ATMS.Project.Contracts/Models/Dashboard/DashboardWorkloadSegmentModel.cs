using ATMS.Project.Contracts.Models.Users;

namespace ATMS.Project.Contracts.Models.Dashboard;

public sealed class DashboardWorkloadSegmentModel
{
    public string Kind { get; init; }

    public PersonModel? Person { get; init; }

    public int Value { get; init; }
}


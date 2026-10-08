using ATMS.Project.Data.Models.Dashboard;

namespace ATMS.Project.Services.Models.Dashboard;

public sealed record DashboardPeriodWindow(
    DateTime GeneratedAt,
    string Period,
    DateOnly FirstDay,
    DateOnly LastDay,
    DashboardDataWindow Data);

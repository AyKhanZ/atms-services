using ATMS.Project.Data.Entities;

namespace ATMS.Project.Data.Models.Dashboard;

public sealed record DashboardActivityRow(HistoryEntry Entry, DashboardActivitySubjectRow Subject);

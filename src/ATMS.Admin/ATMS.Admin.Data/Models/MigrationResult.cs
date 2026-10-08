namespace ATMS.Admin.Data.Models;

public sealed class MigrationResult
{
    public string? ErrorMessage { get; set; }
    public IReadOnlyList<string> AppliedMigrations { get; init; } = [];
    public string? RolledBackMigration { get; init; }
}

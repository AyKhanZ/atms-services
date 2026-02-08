namespace ATMS.Admin.Data.Infrastructure.Migrations;

public class MigrationResult
{
    public string? ErrorMessage { get; set; }
    public IReadOnlyList<string> AppliedMigrations { get; init; } = [];
    public string? RolledBackMigration { get; init; }
}

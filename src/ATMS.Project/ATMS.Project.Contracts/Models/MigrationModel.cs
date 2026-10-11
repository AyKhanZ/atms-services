namespace ATMS.Project.Contracts.Models;

public sealed class MigrationModel
{
    public bool Success => string.IsNullOrWhiteSpace(ErrorMessage);
    public string? ErrorMessage { get; set; }
    public string[] AppliedMigrations { get; init; } = [];
    public string? RolledBackMigration { get; set; }
}

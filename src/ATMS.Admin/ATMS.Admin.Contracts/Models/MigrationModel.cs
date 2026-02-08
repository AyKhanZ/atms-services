namespace ATMS.Admin.Contracts.Models;

public class MigrationModel
{
    public bool Success => string.IsNullOrEmpty(ErrorMessage);
    public string? ErrorMessage { get; set; }
    public string[] AppliedMigrations { get; init; } = [];
    public string? RolledBackMigration { get; set; }
}

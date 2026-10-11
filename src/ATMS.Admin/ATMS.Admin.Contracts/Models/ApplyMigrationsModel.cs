namespace ATMS.Admin.Contracts.Models;

public sealed class ApplyMigrationsModel
{
    public MigrationModel Admin { get; init; } = new();
    public MigrationModel? Project { get; init; }
}

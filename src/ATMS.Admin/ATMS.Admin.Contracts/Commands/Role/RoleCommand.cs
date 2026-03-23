namespace ATMS.Admin.Contracts.Commands.Role;

public class RoleCommand
{
    public required string Name { get; init; }
    public string Description { get; set; }
}

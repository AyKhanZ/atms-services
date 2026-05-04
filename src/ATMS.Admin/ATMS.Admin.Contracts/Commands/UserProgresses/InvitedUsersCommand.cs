namespace ATMS.Admin.Contracts.Commands.UserProgresses;

public class InvitedUsersCommand
{
    public required string Name { get; init; }
    
    public required string Surname { get; init; }
    
    public required string Email { get; init; }
}
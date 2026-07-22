namespace ATMS.Admin.Contracts.Commands.Onboarding;

public class InvitedUserCommand
{
    public required string Name { get; set; }
    
    public required string Surname { get; set; }
    
    public required string Email { get; set; }
}

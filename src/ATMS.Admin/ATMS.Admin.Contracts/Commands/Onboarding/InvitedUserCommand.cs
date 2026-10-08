namespace ATMS.Admin.Contracts.Commands.Onboarding;

public sealed class InvitedUserCommand
{
    public required string Name { get; set; }
    
    public required string Surname { get; set; }
    
    public required string Email { get; set; }
}

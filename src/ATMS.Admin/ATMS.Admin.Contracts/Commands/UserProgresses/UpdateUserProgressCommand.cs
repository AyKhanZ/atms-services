using MediatR;

namespace ATMS.Admin.Contracts.Commands.UserProgresses;

public class UpdateUserProgressCommand : IRequest
{
    public PersonalInfoCommand? PersonalInfoCommand { get; set; }
    
    public string? Password { get; set; }
    
    public List<InvitedUsersCommand>? InvitedUsersCommand { get; set; }
}
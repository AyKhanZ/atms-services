namespace ATMS.Admin.Contracts.Commands.Account;

public class ChangePasswordCommand
{
    public string Email { get; set; }
    public string OldPassword { get; set; }
    public string NewPassword { get; set; }
}

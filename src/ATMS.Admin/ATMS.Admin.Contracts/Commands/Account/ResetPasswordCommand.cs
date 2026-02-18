namespace ATMS.Admin.Contracts.Commands.Account;

public class ResetPasswordCommand
{
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
    public string Token { get; set; }
}

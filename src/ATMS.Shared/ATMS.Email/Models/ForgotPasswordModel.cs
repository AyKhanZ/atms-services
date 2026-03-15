namespace ATMS.Email.Models;

public class ForgotPasswordModel
{
    public string Link { get; set; }
    public DateTime DeadlineOfToken { get; set; }
}
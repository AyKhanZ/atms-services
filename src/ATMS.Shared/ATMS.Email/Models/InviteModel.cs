namespace ATMS.Email.Models;

public class InviteModel
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string Name { get; set; }
    public required string Surname { get; set; }
    public required string Link { get; set; }
    public DateTime DeadlineOfToken { get; set; }
}

namespace ATMS.Admin.Data.Entities;

public class PasswordResetToken : BaseEntity
{
    public string Token { get; set; }
    public DateTime ExpiryDate { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; }
}

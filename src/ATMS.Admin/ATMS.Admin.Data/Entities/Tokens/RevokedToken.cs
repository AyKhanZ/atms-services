namespace ATMS.Admin.Data.Entities.Tokens;

public class RevokedToken : BaseEntity
{
    public Guid UserId { get; set; }
    public string RefreshToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
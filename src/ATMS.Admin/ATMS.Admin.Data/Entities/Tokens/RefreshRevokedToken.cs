using ATMS.Data;

namespace ATMS.Admin.Data.Entities.Tokens;

public class RefreshRevokedToken : BaseEntity
{
    public string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
    
    public Guid UserId { get; set; }
    
    public User User { get; set; }
}

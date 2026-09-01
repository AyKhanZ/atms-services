using ATMS.Data;

namespace ATMS.Admin.Data.Entities.Tokens;

public class UserSession : BaseEntity
{
    public Guid UserId { get; set; }

    public User User { get; set; }

    public Guid FamilyId { get; set; }

    public required string TokenHash { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime FamilyExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }
}

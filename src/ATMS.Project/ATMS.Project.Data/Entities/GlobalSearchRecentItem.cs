namespace ATMS.Project.Data.Entities;

public class GlobalSearchRecentItem
{
    public Guid UserId { get; set; }

    public int ItemType { get; set; }

    public Guid ItemId { get; set; }

    public DateTime OpenedAt { get; set; }
}

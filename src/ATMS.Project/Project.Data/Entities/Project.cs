using ATMS.Data;

namespace Project.Data.Entities;

public class Project : BaseEntity
{
    public string Title { get; set; }
    
    public string Code { get; set; }
    
    public string? Description { get; set; }

    
    public Guid OrganizationId { get; set; }
    
    public Organization Organization { get; set; }


    #region Dictionaries
    
    public int ProjectTypeId { get; set; }
    
    public int StateId { get; set; }
    
    #endregion
    
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    
    private ICollection<Group> Groups { get; set; } = [];
}

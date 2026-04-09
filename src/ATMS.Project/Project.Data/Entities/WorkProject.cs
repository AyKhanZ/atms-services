using ATMS.Data;
using Project.Data.Entities.Dictionaries;

namespace Project.Data.Entities;

public class WorkProject : AuditableEntity
{
    public string Code { get; set; }
    
    public string Title { get; set; }
    
    
    public string? Description { get; set; }

    
    public Guid OrganizationId { get; set; }
    
    public Organization Organization { get; set; }


    #region Dictionaries
    
    public ProjectType ProjectType { get; set; }
    
    public int ProjectTypeId { get; set; }
    
    
    public ProjectKind ProjectKind { get; set; }
    
    public int ProjectKindId { get; set; }
    
    
    public ProjectStatus ProjectStatus { get; set; }
    
    public int ProjectStatusId { get; set; }
    
    #endregion
    
    
    
    public ICollection<WorkProjectParticipant> WorkProjectParticipants { get; set; } = [];
    
    public ICollection<WorkGroup> WorkGroups { get; set; } = [];
}

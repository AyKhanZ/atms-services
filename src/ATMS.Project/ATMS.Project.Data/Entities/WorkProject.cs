using ATMS.Data;
using ATMS.Project.Data.Entities.Dictionaries;

namespace ATMS.Project.Data.Entities;

public class WorkProject : SoftDeletableAuditableEntity<User>
{
    public string Code { get; set; }
    
    public string Title { get; set; }
    
    
    public string? Description { get; set; }

    
    public Guid? OrganizationId { get; set; }
    
    public Organization Organization { get; set; }



    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }



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

    public ICollection<Meeting> Meetings { get; set; } = [];
}

namespace ATMS.Admin.Data.Entities;

public class OrganizationInfo : BaseEntity
{
    public string Name { get; set; }
    public string LogoImagePath { get; set; }

    public Guid UserProgressId { get; set; }
    public UserProgress UserProgress { get; set; }
}

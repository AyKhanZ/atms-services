namespace ATMS.Project.Contracts.Models.Organization;

public class OrganizationItemModel
{
    public Guid Id { get; set; }

    public string Title { get; set; }

    public string Voen { get; set; }

    public string? LogoPath { get; set; }

    public DateTime CreatedAt { get; set; }
}

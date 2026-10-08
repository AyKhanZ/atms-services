namespace ATMS.Admin.Contracts.Models.Organizations;

public sealed class OrganizationModel
{
    public Guid Id { get; set; }
    
    public string Title { get; set; }
    
    public string Voen { get; set; }

    public string? LogoPath { get; set; }
}

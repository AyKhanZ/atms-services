using Microsoft.AspNetCore.Http;

namespace ATMS.Project.Contracts.Commands.Organizations;

public class OrganizationCommand
{
    public required string Title { get; set; }
    
    public required string Voen { get; set; }
    
    public IFormFile? Logo { get; set; }
}
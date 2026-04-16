namespace ATMS.Data.Constants;

public static class RoleIds
{
    // System Roles
    public static readonly Guid Client = Guid.Parse("dc91d07f-2a00-486b-8a90-aa7b4c688de8");
    
    public static readonly Guid ClientManager = Guid.Parse("4c0a7e27-0576-4738-9f73-1d9cc14374a5");
    
    public static readonly Guid Agent = Guid.Parse("58a8f620-1550-41a2-8693-336fd9bbeb53");
    
    
    // Project Roles
    public static readonly Guid ProjectManager = Guid.Parse("869cbfbe-f0ad-4357-b369-71b3ece4a890");
    
    public static readonly Guid BusinessConsultant = Guid.Parse("7b59a306-3455-4d35-bb7d-d7a07e8219ca");
    
    public static readonly Guid Developer = Guid.Parse("51805e71-420c-40c4-a074-76b4f29eee7a");
    
    public static readonly Guid OrgClientManager = Guid.Parse("fa1dac7e-d57c-4e4c-9f71-283566862346");
    
    public static readonly Guid OrgClientViewer = Guid.Parse("6b738142-0c09-47d0-848b-f2d5e411b266");
}
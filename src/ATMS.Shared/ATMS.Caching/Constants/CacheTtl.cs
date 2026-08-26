namespace ATMS.Caching.Constants;

public static class CacheTtl
{
    // Dictionaries (genders, statuses) — change very rarely
    public static readonly TimeSpan Dictionary = TimeSpan.FromMinutes(30);

    // Roles and permissions
    public static readonly TimeSpan Roles = TimeSpan.FromMinutes(15);
    public static readonly TimeSpan Permissions = TimeSpan.FromMinutes(10);
    public static readonly TimeSpan ProjectPermissions = TimeSpan.FromMinutes(5);

    // A regular entity by ID (user, project)
    public static readonly TimeSpan Entity = TimeSpan.FromMinutes(5);
    
    // A regular entity by ID (ticket, tasks)
    public static readonly TimeSpan ActiveItem = TimeSpan.FromMinutes(3);
}

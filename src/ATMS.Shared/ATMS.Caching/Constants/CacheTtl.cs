namespace ATMS.Caching.Constants;

public static class CacheTtl
{
    // Dictionaries (genders, statuses) — change very rarely
    public static readonly TimeSpan Dictionary = TimeSpan.FromHours(6);

    // Roles and permissions
    public static readonly TimeSpan Roles = TimeSpan.FromHours(1);
    public static readonly TimeSpan Permissions = TimeSpan.FromMinutes(30);

    // A regular entity by ID (user, project)
    public static readonly TimeSpan Entity = TimeSpan.FromMinutes(5);
    
    // A regular entity by ID (ticket, tasks)
    public static readonly TimeSpan ActiveItem = TimeSpan.FromMinutes(3);
}
namespace ATMS.Caching.Constants;

public static class CacheKeys
{
    public static class Admin
    {
        public static string UserById(Guid id, string language) => $"user:{id}:{language}";
        
        public static string MeById(Guid id) => $"user:me:{id}";
        
        public static string UserRoles(Guid userId) => $"user:{userId}:roles";
        
        public static string UserPermissions(Guid userId) => $"user:{userId}:permissions";
        
        public static string RoleById(Guid id) => $"role:{id}";
        
        public const string AllRoles = "roles:all";
        
        public static string AllPermissions(string language) => $"permissions:{language}";
        
        
        // Dictionaries
        public static string AllGenders(string language) => $"dict:genders:{language}";
        
        public static string AllUserStatuses(string language) => $"dict:user-statuses:{language}";
        
        public static string AllMaritalStatuses(string language) => $"dict:marital-statuses:{language}";
    }

    public static class Project
    {
        public static string ProjectById(Guid id) => $"work-project:{id}";

        public static string TicketById(Guid id) => $"work-ticket:{id}";
        
        public static string TaskById(Guid id) => $"work-task:{id}";
        
        // Dictionaries
        public static string AllProjectRoles(string language) => $"dict:project-roles:{language}";
        
        public static string AllProjectKinds(string language) => $"dict:project-kinds:{language}";
        
        public static string AllProjectStatuses(string language) => $"dict:project-statuses:{language}";
        
        public static string AllProjectTypes(string language) => $"dict:project-types:{language}";
        
        public static string AllWorkGroupStatuses(string language) => $"dict:work-group-statuses:{language}";
        
        public static string AllWorkItemPriorities(string language) => $"dict:work-item-priorities:{language}";
        
        public static string AllWorkTaskStatuses(string language) => $"dict:work-task-statuses:{language}";
        
        public static string AllWorkTicketStatuses(string language) => $"dict:work-ticket-statuses:{language}";
        
        public static string AllWorkTicketTypes(string language) => $"dict:work-ticket-types:{language}";
    }
}

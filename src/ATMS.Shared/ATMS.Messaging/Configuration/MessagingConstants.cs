namespace ATMS.Messaging.Configuration;

public static class MessagingConstants
{
    public static class Exchanges
    {
        public const string UserEvents = "atms.user.events";
        
        public const string ProjectEvents = "atms.project.events";
    }

    public static class Queues
    {
        // Invite User
        public const string UserInvited = "atms.user.invited";
        public const string UserInvitedRetry = "atms.user.invited.retry";
        public const string UserInvitedDead = "atms.user.invited.dead";
        
        // Update User
        public const string ProjectUserUpdated = "atms.project.user.updated";
        public const string ProjectUserUpdatedRetry = "atms.project.user.updated.retry";
        public const string ProjectUserUpdatedDead = "atms.project.user.updated.dead";
        
        // Create User
        public const string ProjectUserCreated = "atms.project.user.created";
        public const string ProjectUserCreatedRetry = "atms.project.user.created.retry";
        public const string ProjectUserCreatedDead = "atms.project.user.created.dead";
        
        // Update Organization
        // public const string AdminOrganizationArchived = "atms.admin.organization.archived";
        // public const string AdminOrganizationArchivedRetry = "atms.admin.organization.archived.retry";
        // public const string AdminOrganizationArchivedDead = "atms.admin.organization.archived.dead";
    }

    public static class RoutingKeys
    {
        // User
        public const string UserCreated = "user.created";
        public const string UserUpdated = "user.updated";
        public const string UserInvited = "user.invited";
        // public const string OrganizationArchived = "organization.archived";
    }
}
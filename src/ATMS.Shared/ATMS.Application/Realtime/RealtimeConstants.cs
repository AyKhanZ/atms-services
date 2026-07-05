namespace ATMS.Application.Realtime;

public static class RealtimeConstants
{
    public const string HubPath = "/hubs/realtime";

    public static class Groups
    {
        public const string Admins = "admins";

        public static string Organization(Guid organizationId) => $"organization:{organizationId}";

        public static string Project(Guid projectId) => $"project:{projectId}";
    }

    public static class Events
    {
        public const string UserChanged = "userChanged";
        public const string OrganizationChanged = "organizationChanged";
        public const string ProjectChanged = "projectChanged";
        public const string TicketChanged = "ticketChanged";
        public const string TaskChanged = "taskChanged";
        public const string NotificationChanged = "notificationChanged";
    }
}

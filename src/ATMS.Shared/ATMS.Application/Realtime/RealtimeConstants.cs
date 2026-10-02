namespace ATMS.Application.Realtime;

public static class RealtimeConstants
{
    public const string HubPath = "/hubs/realtime";

    public static class Groups
    {
        public static string Project(Guid projectId) => $"project:{projectId}";

        public static string Task(Guid taskId) => $"task:{taskId}";
    }
}

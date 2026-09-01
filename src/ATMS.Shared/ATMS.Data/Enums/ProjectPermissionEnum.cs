namespace ATMS.Data.Enums;

public enum ProjectPermissionEnum
{
    ProjectView = 1,
    ProjectEdit = 2,

    TicketEdit = 5,
    TicketDelete = 6,
    
    TaskEdit = 8,
    TaskDelete = 9,
    
    CommentEdit = 11,
    CommentDelete = 12,
    
    NotificationEdit = 14,
    NotificationDelete = 15,

    ParticipantEdit = 26,
    ParticipantDelete = 27,
    ParticipantInviteClient = 28,
    ParticipantInviteEmployee = 33,

    TicketCreate = 29,
    TaskCreate = 30
}

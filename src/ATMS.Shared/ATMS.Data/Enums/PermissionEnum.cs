namespace ATMS.Data.Enums;

public enum PermissionEnum
{
    RoleView = 1,
    RoleEdit,
    RoleDelete,

    UserView,
    UserEdit,
    UserDelete,

    ProjectView,
    ProjectEdit,

    NotificationView = 10,
    NotificationEdit,
    NotificationDelete,
    
    CommentView = 13,
    CommentEdit,
    CommentDelete,
    
    OrganizationView = 16,
    OrganizationEdit,
    OrganizationDelete,
}

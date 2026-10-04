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

    CommentView = 13,
    CommentEdit,
    CommentDelete,
    
    OrganizationView = 16,
    OrganizationEdit,
    OrganizationDelete,
}

using ATMS.Data.Constants;
using ATMS.Data.Enums;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Project.Services.Tests.Security;

// What each project role may do, as seeded: 13-comments and 06-api-and-data in one table.
public class RolePermissionSeedTest
{
    private static readonly ProjectPermissionEnum[] TeamMember =
    [
        ProjectPermissionEnum.ProjectView,
        ProjectPermissionEnum.TicketCreate,
        ProjectPermissionEnum.TicketEdit,
        ProjectPermissionEnum.TicketDelete,
        ProjectPermissionEnum.TaskCreate,
        ProjectPermissionEnum.TaskEdit,
        ProjectPermissionEnum.TaskDelete,
        ProjectPermissionEnum.CommentEdit
    ];

    public static TheoryData<Guid, ProjectPermissionEnum[]> Roles => new()
    {
        { RoleIds.Developer, TeamMember },
        { RoleIds.BusinessConsultant, TeamMember },
        {
            RoleIds.OrgClientManager,
            [
                ProjectPermissionEnum.ProjectView,
                ProjectPermissionEnum.TicketCreate,
                ProjectPermissionEnum.CommentEdit,
                ProjectPermissionEnum.ParticipantInviteClient
            ]
        },
        { RoleIds.OrgClientViewer, [ProjectPermissionEnum.ProjectView, ProjectPermissionEnum.CommentEdit] }
    };

    [Theory]
    [MemberData(nameof(Roles))]
    public void EveryRoleButTheManager_WritesAndDeletesOnlyItsOwnComments(
        Guid roleId,
        ProjectPermissionEnum[] expected)
    {
        Assert.Equal(expected.Order(), PermissionsOf(roleId).Order());
        Assert.DoesNotContain(ProjectPermissionEnum.CommentDelete, PermissionsOf(roleId));
    }

    [Fact]
    public void TheProjectManager_DeletesAnyCommentInTheProject()
    {
        Assert.Contains(ProjectPermissionEnum.CommentDelete, PermissionsOf(RoleIds.ProjectManager));
    }

    private static ProjectPermissionEnum[] PermissionsOf(Guid roleId)
    {
        using var context = new ProjectDbContext(
            new DbContextOptionsBuilder<ProjectDbContext>().UseNpgsql("Host=localhost;Database=model_only").Options);

        return context.GetService<IDesignTimeModel>().Model.FindEntityType(typeof(RolePermission))!
            .GetSeedData()
            .Where(row => (Guid)row[nameof(RolePermission.RoleId)]! == roleId)
            .Select(row => (ProjectPermissionEnum)(int)row[nameof(RolePermission.PermissionId)]!)
            .ToArray();
    }
}

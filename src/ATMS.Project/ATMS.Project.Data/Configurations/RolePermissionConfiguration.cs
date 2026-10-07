using ATMS.Data.Constants;
using ATMS.Data.Enums;
using ATMS.Project.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.Configurations;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    private static readonly IReadOnlyDictionary<Guid, ProjectPermissionEnum[]> PermissionsByRole =
        new Dictionary<Guid, ProjectPermissionEnum[]>
        {
            [RoleIds.ProjectManager] =
            [
                ProjectPermissionEnum.ProjectView,
                ProjectPermissionEnum.ProjectEdit,
                ProjectPermissionEnum.TicketCreate,
                ProjectPermissionEnum.TicketEdit,
                ProjectPermissionEnum.TicketDelete,
                ProjectPermissionEnum.TaskCreate,
                ProjectPermissionEnum.TaskEdit,
                ProjectPermissionEnum.TaskDelete,
                ProjectPermissionEnum.CommentEdit,
                ProjectPermissionEnum.CommentDelete,
                ProjectPermissionEnum.ParticipantEdit,
                ProjectPermissionEnum.ParticipantDelete,
                ProjectPermissionEnum.ParticipantInviteClient,
                ProjectPermissionEnum.ParticipantInviteEmployee
            ],
            // The team works the tickets and tasks; only the project manager deletes someone else's comment.
            [RoleIds.BusinessConsultant] = TeamMember(),
            [RoleIds.Developer] = TeamMember(),
            [RoleIds.OrgClientManager] =
            [
                ProjectPermissionEnum.ProjectView,
                ProjectPermissionEnum.TicketCreate,
                ProjectPermissionEnum.CommentEdit,
                ProjectPermissionEnum.ParticipantInviteClient,
                ProjectPermissionEnum.ParticipantDeleteClient
            ],
            [RoleIds.OrgClientViewer] = [ProjectPermissionEnum.ProjectView, ProjectPermissionEnum.CommentEdit]
        };

    private static ProjectPermissionEnum[] TeamMember() =>
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

    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.HasKey(rp => new { rp.PermissionId, rp.RoleId });
    
        
        builder.HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(rp => rp.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasData(PermissionsByRole
            .SelectMany(pair => pair.Value.Select(permission => new
            {
                PermissionId = (int)permission,
                RoleId = pair.Key
            }))
            .Cast<object>()
            .ToArray());
    }
}

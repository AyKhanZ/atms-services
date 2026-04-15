using ATMS.Admin.Data.Entities;
using ATMS.Data.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Admin.Data.Configurations;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.HasKey(x => new { x.PermissionId, x.RoleId });

        builder.HasIndex(x => x.RoleId);

        builder.HasIndex(x => x.PermissionId);


        builder.HasOne(x => x.Permission)
            .WithMany(x => x.RolePermissions)
            .HasForeignKey(x => x.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Role)
            .WithMany(x => x.RolePermissions)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);


        builder.HasData(
            // Client
            new { PermissionId = PermissionIds.RoleView, RoleId = RoleIds.Client },
            new { PermissionId = PermissionIds.UserView, RoleId = RoleIds.Client },
            new { PermissionId = PermissionIds.ProjectView, RoleId = RoleIds.Client },
            new { PermissionId = PermissionIds.NotificationView, RoleId = RoleIds.Client },
            new { PermissionId = PermissionIds.CommentView, RoleId = RoleIds.Client },
            // Client Manager
            new { PermissionId = PermissionIds.RoleView, RoleId = RoleIds.ClientManager },
            new { PermissionId = PermissionIds.UserView, RoleId = RoleIds.ClientManager },
            new { PermissionId = PermissionIds.ProjectView, RoleId = RoleIds.ClientManager },
            new { PermissionId = PermissionIds.NotificationView, RoleId = RoleIds.ClientManager },
            new { PermissionId = PermissionIds.CommentView, RoleId = RoleIds.ClientManager },
            new { PermissionId = PermissionIds.CommentEdit, RoleId = RoleIds.ClientManager },
            new { PermissionId = PermissionIds.CommentDelete, RoleId = RoleIds.ClientManager },
            // Agent
            new { PermissionId = PermissionIds.RoleView, RoleId = RoleIds.Agent },
            new { PermissionId = PermissionIds.UserView, RoleId = RoleIds.Agent },
            new { PermissionId = PermissionIds.ProjectView, RoleId = RoleIds.Agent },
            new { PermissionId = PermissionIds.ProjectEdit, RoleId = RoleIds.Agent },
            new { PermissionId = PermissionIds.NotificationView, RoleId = RoleIds.Agent },
            new { PermissionId = PermissionIds.CommentView, RoleId = RoleIds.Agent },
            new { PermissionId = PermissionIds.CommentEdit, RoleId = RoleIds.Agent },
            new { PermissionId = PermissionIds.CommentDelete, RoleId = RoleIds.Agent }
        );
    }
}
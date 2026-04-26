using ATMS.Admin.Data.Entities;
using ATMS.Data.Constants;
using ATMS.Data.Enums;
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
            new { PermissionId = (int)PermissionEnum.RoleView, RoleId = RoleIds.Client },
            new { PermissionId = (int)PermissionEnum.UserView, RoleId = RoleIds.Client },
            new { PermissionId = (int)PermissionEnum.ProjectView, RoleId = RoleIds.Client },
            new { PermissionId = (int)PermissionEnum.NotificationView, RoleId = RoleIds.Client },
            new { PermissionId = (int)PermissionEnum.CommentView, RoleId = RoleIds.Client },
            // Client Manager
            new { PermissionId = (int)PermissionEnum.RoleView, RoleId = RoleIds.ClientManager },
            new { PermissionId = (int)PermissionEnum.UserView, RoleId = RoleIds.ClientManager },
            new { PermissionId = (int)PermissionEnum.ProjectView, RoleId = RoleIds.ClientManager },
            new { PermissionId = (int)PermissionEnum.NotificationView, RoleId = RoleIds.ClientManager },
            new { PermissionId = (int)PermissionEnum.CommentView, RoleId = RoleIds.ClientManager },
            new { PermissionId = (int)PermissionEnum.CommentEdit, RoleId = RoleIds.ClientManager },
            new { PermissionId = (int)PermissionEnum.CommentDelete, RoleId = RoleIds.ClientManager },
            // Agent
            new { PermissionId = (int)PermissionEnum.RoleView, RoleId = RoleIds.Agent },
            new { PermissionId = (int)PermissionEnum.UserView, RoleId = RoleIds.Agent },
            new { PermissionId = (int)PermissionEnum.ProjectView, RoleId = RoleIds.Agent },
            new { PermissionId = (int)PermissionEnum.ProjectEdit, RoleId = RoleIds.Agent },
            new { PermissionId = (int)PermissionEnum.NotificationView, RoleId = RoleIds.Agent },
            new { PermissionId = (int)PermissionEnum.CommentView, RoleId = RoleIds.Agent },
            new { PermissionId = (int)PermissionEnum.CommentEdit, RoleId = RoleIds.Agent },
            new { PermissionId = (int)PermissionEnum.CommentDelete, RoleId = RoleIds.Agent }
        );
    }
}
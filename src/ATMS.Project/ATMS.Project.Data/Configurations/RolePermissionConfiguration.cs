using ATMS.Data.Constants;
using ATMS.Data.Enums;
using ATMS.Project.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.Configurations;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
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
        
        
        builder.HasData(
            // Project Manager
            new { PermissionId = (int)ProjectPermissionEnum.ProjectView, RoleId = RoleIds.ProjectManager },
            new { PermissionId = (int)ProjectPermissionEnum.ProjectEdit, RoleId = RoleIds.ProjectManager },
            new { PermissionId = (int)ProjectPermissionEnum.ProjectDelete, RoleId = RoleIds.ProjectManager },
            new { PermissionId = (int)ProjectPermissionEnum.GroupView, RoleId = RoleIds.ProjectManager },
            new { PermissionId = (int)ProjectPermissionEnum.GroupEdit, RoleId = RoleIds.ProjectManager },
            new { PermissionId = (int)ProjectPermissionEnum.GroupDelete, RoleId = RoleIds.ProjectManager },
            new { PermissionId = (int)ProjectPermissionEnum.TicketView, RoleId = RoleIds.ProjectManager },
            new { PermissionId = (int)ProjectPermissionEnum.TicketEdit, RoleId = RoleIds.ProjectManager },
            new { PermissionId = (int)ProjectPermissionEnum.TicketDelete, RoleId = RoleIds.ProjectManager },
            new { PermissionId = (int)ProjectPermissionEnum.TaskView, RoleId = RoleIds.ProjectManager },
            new { PermissionId = (int)ProjectPermissionEnum.TaskEdit, RoleId = RoleIds.ProjectManager },
            new { PermissionId = (int)ProjectPermissionEnum.TaskDelete, RoleId = RoleIds.ProjectManager },
            new { PermissionId = (int)ProjectPermissionEnum.CommentView, RoleId = RoleIds.ProjectManager },
            new { PermissionId = (int)ProjectPermissionEnum.CommentEdit, RoleId = RoleIds.ProjectManager },
            new { PermissionId = (int)ProjectPermissionEnum.CommentDelete, RoleId = RoleIds.ProjectManager },
            new { PermissionId = (int)ProjectPermissionEnum.NotificationView, RoleId = RoleIds.ProjectManager },
            // Business Consultant
            new { PermissionId = (int)ProjectPermissionEnum.ProjectView, RoleId = RoleIds.BusinessConsultant },
            new { PermissionId = (int)ProjectPermissionEnum.ProjectEdit, RoleId = RoleIds.BusinessConsultant },
            new { PermissionId = (int)ProjectPermissionEnum.ProjectDelete, RoleId = RoleIds.BusinessConsultant },
            new { PermissionId = (int)ProjectPermissionEnum.GroupView, RoleId = RoleIds.BusinessConsultant },
            new { PermissionId = (int)ProjectPermissionEnum.GroupEdit, RoleId = RoleIds.BusinessConsultant },
            new { PermissionId = (int)ProjectPermissionEnum.GroupDelete, RoleId = RoleIds.BusinessConsultant },
            new { PermissionId = (int)ProjectPermissionEnum.TicketView, RoleId = RoleIds.BusinessConsultant },
            new { PermissionId = (int)ProjectPermissionEnum.TicketEdit, RoleId = RoleIds.BusinessConsultant },
            new { PermissionId = (int)ProjectPermissionEnum.TicketDelete, RoleId = RoleIds.BusinessConsultant },
            new { PermissionId = (int)ProjectPermissionEnum.TaskView, RoleId = RoleIds.BusinessConsultant },
            new { PermissionId = (int)ProjectPermissionEnum.TaskEdit, RoleId = RoleIds.BusinessConsultant },
            new { PermissionId = (int)ProjectPermissionEnum.TaskDelete, RoleId = RoleIds.BusinessConsultant },
            new { PermissionId = (int)ProjectPermissionEnum.CommentView, RoleId = RoleIds.BusinessConsultant },
            new { PermissionId = (int)ProjectPermissionEnum.CommentEdit, RoleId = RoleIds.BusinessConsultant },
            new { PermissionId = (int)ProjectPermissionEnum.CommentDelete, RoleId = RoleIds.BusinessConsultant },
            new { PermissionId = (int)ProjectPermissionEnum.NotificationView, RoleId = RoleIds.BusinessConsultant },
            // Developer
            new { PermissionId = (int)ProjectPermissionEnum.ProjectView, RoleId = RoleIds.Developer },
            new { PermissionId = (int)ProjectPermissionEnum.ProjectEdit, RoleId = RoleIds.Developer },
            new { PermissionId = (int)ProjectPermissionEnum.ProjectDelete, RoleId = RoleIds.Developer },
            new { PermissionId = (int)ProjectPermissionEnum.GroupView, RoleId = RoleIds.Developer },
            new { PermissionId = (int)ProjectPermissionEnum.GroupEdit, RoleId = RoleIds.Developer },
            new { PermissionId = (int)ProjectPermissionEnum.GroupDelete, RoleId = RoleIds.Developer },
            new { PermissionId = (int)ProjectPermissionEnum.TicketView, RoleId = RoleIds.Developer },
            new { PermissionId = (int)ProjectPermissionEnum.TicketEdit, RoleId = RoleIds.Developer },
            new { PermissionId = (int)ProjectPermissionEnum.TicketDelete, RoleId = RoleIds.Developer },
            new { PermissionId = (int)ProjectPermissionEnum.TaskView, RoleId = RoleIds.Developer },
            new { PermissionId = (int)ProjectPermissionEnum.TaskEdit, RoleId = RoleIds.Developer },
            new { PermissionId = (int)ProjectPermissionEnum.TaskDelete, RoleId = RoleIds.Developer },
            new { PermissionId = (int)ProjectPermissionEnum.CommentView, RoleId = RoleIds.Developer },
            new { PermissionId = (int)ProjectPermissionEnum.CommentEdit, RoleId = RoleIds.Developer },
            new { PermissionId = (int)ProjectPermissionEnum.CommentDelete, RoleId = RoleIds.Developer },
            new { PermissionId = (int)ProjectPermissionEnum.NotificationView, RoleId = RoleIds.Developer },
            // Org Client Manager
            new { PermissionId = (int)ProjectPermissionEnum.ProjectView, RoleId = RoleIds.OrgClientManager },
            new { PermissionId = (int)ProjectPermissionEnum.GroupView, RoleId = RoleIds.OrgClientManager },
            new { PermissionId = (int)ProjectPermissionEnum.TicketView, RoleId = RoleIds.OrgClientManager },
            new { PermissionId = (int)ProjectPermissionEnum.TaskView, RoleId = RoleIds.OrgClientManager },
            new { PermissionId = (int)ProjectPermissionEnum.CommentView, RoleId = RoleIds.OrgClientManager },
            new { PermissionId = (int)ProjectPermissionEnum.NotificationView, RoleId = RoleIds.OrgClientManager },
            new { PermissionId = (int)ProjectPermissionEnum.CommentEdit, RoleId = RoleIds.OrgClientManager },
            new { PermissionId = (int)ProjectPermissionEnum.CommentDelete, RoleId = RoleIds.OrgClientManager },
            new { PermissionId = (int)ProjectPermissionEnum.UserInvite, RoleId = RoleIds.OrgClientManager },
            // Org Client Viewer
            new { PermissionId = (int)ProjectPermissionEnum.ProjectView, RoleId = RoleIds.OrgClientViewer },
            new { PermissionId = (int)ProjectPermissionEnum.GroupView, RoleId = RoleIds.OrgClientViewer },
            new { PermissionId = (int)ProjectPermissionEnum.TicketView, RoleId = RoleIds.OrgClientViewer },
            new { PermissionId = (int)ProjectPermissionEnum.TaskView, RoleId = RoleIds.OrgClientViewer },
            new { PermissionId = (int)ProjectPermissionEnum.CommentView, RoleId = RoleIds.OrgClientViewer },
            new { PermissionId = (int)ProjectPermissionEnum.NotificationView, RoleId = RoleIds.OrgClientViewer },
            new { PermissionId = (int)ProjectPermissionEnum.CommentEdit, RoleId = RoleIds.OrgClientViewer },
            new { PermissionId = (int)ProjectPermissionEnum.CommentDelete, RoleId = RoleIds.OrgClientViewer }
        );
    }
}

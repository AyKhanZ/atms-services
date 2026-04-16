using ATMS.Data.Constants;
using ATMS.Project.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasIndex(e => e.Name)
            .IsUnique();
        
        builder.Property(e => e.Name)
            .IsRequired();

        builder.Property(e => e.IsDeleted)
            .HasDefaultValue(false);
        
        
        builder.HasData(
            new { Id = RoleIds.ProjectManager, Name = "Project Manager", Description = "Project Manager Role" },
            new { Id = RoleIds.BusinessConsultant, Name = "Business Consultant", Description = "Business Consultant Role" },
            new { Id = RoleIds.Developer, Name = "Developer", Description = "Developer Role" },
            new { Id = RoleIds.OrgClientManager, Name = "Client Manager", Description = "Client Manager Role" },
            new { Id = RoleIds.OrgClientViewer, Name = "Client Viewer", Description = "Client Viewer Role" }
        );
    }
}

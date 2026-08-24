using ATMS.Data.Constants;
using ATMS.Project.Data.Entities;
using ATMS.Data.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.Configurations;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.HasIndex(e => e.Title)
            .IsUnique();
        
        builder.HasIndex(e => e.Voen)
            .IsUnique();
            
        builder.HasIndex(u => u.CreatedAt);
        
        
        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(e => e.Voen)
            .IsRequired()
            .HasMaxLength(20);
        
        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.LogoPath)
            .HasDefaultValue(DefaultValues.OrganizationLogo);
        
        
        builder.HasMany(o => o.Users)
            .WithOne(u => u.Organization)
            .HasForeignKey(u => u.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.WorkProjects)
            .WithOne(p => p.Organization)
            .HasForeignKey(p => p.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ConfigureSoftDeletableAuditUserRelationships<Organization, User>();
    }
}

using ATMS.Project.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(e => e.Email)
            .IsUnique();

        builder.Property(e => e.Email)
            .IsRequired();
            
        builder.Property(e => e.Name)
            .IsRequired();
            
        builder.Property(e => e.Surname)
            .IsRequired();
            
        builder.Property(e => e.UserTypeId)
            .IsRequired();
    }
}

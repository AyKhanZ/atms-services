using ATMS.Project.Data.Entities;
using ATMS.Data.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.Configurations;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        // The discussion lists deleted comments too, as placeholders, so the page index covers every
        // row; the count of live ones reads the same index and skips the few deleted.
        builder.HasIndex(e => new { e.OwnerType, e.OwnerId, e.CreatedAt, e.Id })
            .IsDescending(false, false, true, true);

        builder.Property(e => e.OwnerType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(e => e.Text)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.CreatedById)
            .IsRequired();

        builder.ConfigureSoftDeletableAuditUserRelationships<Comment, User>();
    }
}

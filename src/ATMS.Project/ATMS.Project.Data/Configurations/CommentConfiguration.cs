using ATMS.Project.Data.Entities;
using ATMS.Data.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.Configurations;

public sealed class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        // deleted comments are listed too (as placeholders), so the index covers all rows
        builder.HasIndex(e => new { e.OwnerType, e.OwnerId, e.CreatedAt, e.Id })
            .IsDescending(false, false, true, true);

        builder.Property(e => e.OwnerType)
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

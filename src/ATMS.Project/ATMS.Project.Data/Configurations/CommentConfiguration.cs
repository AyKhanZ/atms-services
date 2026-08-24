using ATMS.Project.Data.Entities;
using ATMS.Data.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.Configurations;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.HasIndex(e => new { e.OwnerType, e.OwnerId, e.CreatedAt });

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

        builder.HasOne(e => e.ParentComment)
            .WithMany(e => e.Replies)
            .HasForeignKey(e => e.ParentCommentId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.ConfigureSoftDeletableAuditUserRelationships<Comment, User>();
    }
}

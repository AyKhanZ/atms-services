using ATMS.Project.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.Configurations;

public class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> builder)
    {
        builder.ToTable("Attachments");

        builder.HasIndex(e => new { e.OwnerType, e.OwnerId, e.CreatedAt });

        builder.Property(e => e.OwnerType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(e => e.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.RelativePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.CreatedById)
            .IsRequired();

        builder.HasOne(e => e.Comment)
            .WithMany(e => e.Attachments)
            .HasForeignKey(e => e.CommentId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

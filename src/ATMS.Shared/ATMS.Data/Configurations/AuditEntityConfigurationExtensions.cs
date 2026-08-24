using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Data.Configurations;

public static class AuditEntityConfigurationExtensions
{
    public static void ConfigureAuditUserRelationships<TEntity, TUser>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : AuditableEntity<TUser>
        where TUser : class
    {
        builder.HasOne(e => e.CreatedBy)
            .WithMany()
            .HasForeignKey(e => e.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.UpdatedBy)
            .WithMany()
            .HasForeignKey(e => e.UpdatedById)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public static void ConfigureSoftDeletableAuditUserRelationships<TEntity, TUser>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : SoftDeletableAuditableEntity<TUser>
        where TUser : class
    {
        builder.ConfigureAuditUserRelationships<TEntity, TUser>();

        builder.HasOne(e => e.DeletedBy)
            .WithMany()
            .HasForeignKey(e => e.DeletedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

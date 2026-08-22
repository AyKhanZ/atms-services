using ATMS.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ATMS.Data;

public static class ChangeTrackerAuditExtensions
{
    public static void ApplyAuditMetadata(this ChangeTracker changeTracker, Guid? userId)
    {
        var timestamp = DateTime.UtcNow;
        foreach (var entry in changeTracker.Entries<IAuditable>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = timestamp;
            }

            if (entry.State != EntityState.Modified)
            {
                continue;
            }

            entry.Entity.UpdatedAt = timestamp;
            if (userId.HasValue)
            {
                entry.Entity.UpdatedById = userId;
            }
        }
    }
}

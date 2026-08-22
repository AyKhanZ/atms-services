using ATMS.Data;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Project.Services.Tests;

public sealed class ChangeTrackerAuditExtensionsTest
{
    [Fact]
    public void ApplyAuditMetadata_SetsActorAndTimestampForModifiedEntity()
    {
        using var context = CreateContext();
        var actorId = Guid.NewGuid();
        var organization = CreateOrganization();
        context.Attach(organization);
        context.Entry(organization).State = EntityState.Modified;

        context.ChangeTracker.ApplyAuditMetadata(actorId);

        Assert.Equal(actorId, organization.UpdatedById);
        Assert.NotNull(organization.UpdatedAt);
    }

    [Fact]
    public void ApplyAuditMetadata_DoesNotEraseExistingActorForBackgroundUpdate()
    {
        using var context = CreateContext();
        var existingActorId = Guid.NewGuid();
        var organization = CreateOrganization();
        organization.UpdatedById = existingActorId;
        context.Attach(organization);
        context.Entry(organization).State = EntityState.Modified;

        context.ChangeTracker.ApplyAuditMetadata(null);

        Assert.Equal(existingActorId, organization.UpdatedById);
        Assert.NotNull(organization.UpdatedAt);
    }

    private static ProjectDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ProjectDbContext>()
            .UseNpgsql("Host=localhost;Database=audit-tests;Username=test;Password=test")
            .Options;
        return new ProjectDbContext(options);
    }

    private static Organization CreateOrganization() =>
        new()
        {
            Id = Guid.NewGuid(),
            Title = "Test organization",
            Voen = "1234567890",
            LogoPath = "/logo.png",
        };
}

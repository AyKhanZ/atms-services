using Microsoft.EntityFrameworkCore;
using Project.Data.Entities.Dictionaries;

namespace Project.Data.DbContexts;

public class ProjectDbContext : DbContext
{
    public ProjectDbContext() { }
    public ProjectDbContext(DbContextOptions<ProjectDbContext> options)
        : base(options) { }
    
    #region Dictionaries
    public DbSet<ProjectType> ProjectTypes { get; set; }
    public DbSet<ProjectTypeTranslation> ProjectTypeTranslations { get; set; }
    
    public DbSet<State> State { get; set; }
    public DbSet<StateTranslation> StateTranslations { get; set; }
    
    public DbSet<Permission> Permission { get; set; }
    public DbSet<PermissionTranslation> PermissionTranslations { get; set; }
    #endregion
    
    public DbSet<Entities.Project> Projects { get; set; }
    public DbSet<Entities.Group> Groups { get; set; }
    public DbSet<Entities.Ticket> Tickets { get; set; }
    
    public DbSet<Entities.Member> Members { get; set; }
    public DbSet<Entities.MemberRole> MemberRoles { get; set; }
    
    public DbSet<Entities.User> Users { get; set; }
    public DbSet<Entities.Organization> Organizations { get; set; }
    
    public DbSet<Entities.Role> Roles { get; set; }
    public DbSet<Entities.RolePermission> RolePermissions { get; set; }
    
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5433;Database=atms_project;Username=admin;Password=p@ssw0rd!");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Entities.Project>(entity =>
        {
            entity.HasIndex(e => e.Title).IsUnique();
            entity.HasIndex(e => e.Code).IsUnique();
            
            entity.Property(e => e.Title).IsRequired();
            entity.Property(e => e.Code).IsRequired();

            entity.Property(e => e.CreatedAt).IsRequired();

            entity.Property(u => u.ProjectTypeId)
                .HasDefaultValue(1)
                .IsRequired();

            entity.Property(u => u.StateId)
                .HasDefaultValue(1)
                .IsRequired();
            
            entity.HasOne(p => p.Organization)
                .WithMany(o => o.Projects)
                .HasForeignKey(o => o.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        modelBuilder.Entity<Entities.Group>(entity =>
        {
            entity.HasIndex(e => e.Title).IsUnique();
            
            entity.Property(e => e.Title).IsRequired();

            entity.Property(e => e.CreatedAt).IsRequired();
            
            ////// add!
        });
        
        modelBuilder.Entity<Entities.Organization>(entity =>
        {
            entity.HasIndex(e => e.Title).IsUnique();
            entity.HasIndex(e => e.Voen).IsUnique();
            
            entity.Property(e => e.Title).IsRequired();
            entity.Property(e => e.Voen).IsRequired();

            entity.Property(e => e.LogoPath).HasDefaultValue("logo path");
        });
        
        #region Dictionaries
        modelBuilder.Entity<State>(entity =>
        {
            entity.HasIndex(p => p.Code).IsUnique();
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();

            entity.HasMany(p => p.Translations)
                .WithOne(t => t.State)
                .HasForeignKey(t => t.StateId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<StateTranslation>(entity =>
        {
            entity.HasIndex(t => new { t.StateId, t.Language }).IsUnique();
            entity.Property(t => t.Language).HasMaxLength(5).IsRequired();
            entity.Property(t => t.Name).HasMaxLength(100).IsRequired();
        });
        
        modelBuilder.Entity<ProjectType>(entity =>
        {
            entity.HasIndex(p => p.Code).IsUnique();
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();

            entity.HasMany(p => p.Translations)
                .WithOne(t => t.ProjectType)
                .HasForeignKey(t => t.ProjectTypeId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<PermissionTranslation>(entity =>
        {
            entity.HasIndex(t => new { t.PermissionId, t.Language }).IsUnique();
            entity.Property(t => t.Language).HasMaxLength(5).IsRequired();
            entity.Property(t => t.Name).HasMaxLength(100).IsRequired();
        });
        
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasIndex(p => p.Code).IsUnique();
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();

            entity.HasMany(p => p.Translations)
                .WithOne(t => t.Permission)
                .HasForeignKey(t => t.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<ProjectTypeTranslation>(entity =>
        {
            entity.HasIndex(t => new { t.ProjectTypeId, t.Language }).IsUnique();
            entity.Property(t => t.Language).HasMaxLength(5).IsRequired();
            entity.Property(t => t.Name).HasMaxLength(100).IsRequired();
        });
        #endregion
    }
}

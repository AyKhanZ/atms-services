using Microsoft.EntityFrameworkCore;
using Project.Data.Entities;
using Project.Data.Entities.Dictionaries;

namespace Project.Data.DbContexts;

public class ProjectDbContext : DbContext
{
    public ProjectDbContext() { }
    public ProjectDbContext(DbContextOptions<ProjectDbContext> options)
        : base(options) { }
    
    #region Dictionaries
    public DbSet<WorkTicketType> WorkTicketTypes { get; set; }
    public DbSet<WorkTicketTypeTranslation> WorkTicketTypeTranslations { get; set; }
    
    public DbSet<WorkTicketStatus> WorkTicketStatuses { get; set; }
    public DbSet<WorkTicketStatusTranslation> WorkTicketStatusTranslations { get; set; }
    
    public DbSet<WorkTaskStatus> WorkTaskStatuses { get; set; }
    public DbSet<WorkTaskStatusTranslation> WorkTaskStatusTranslations { get; set; }
    
    public DbSet<WorkGroupStatus> WorkGroupStatuses { get; set; }
    public DbSet<WorkGroupStatusTranslation> WorkGroupStatusTranslations { get; set; }
    
    public DbSet<WorkItemPriority> WorkItemPriorities { get; set; }
    public DbSet<WorkItemPriorityTranslation> WorkItemPriorityTranslations { get; set; }
    
    public DbSet<ProjectType> ProjectTypes { get; set; }
    public DbSet<ProjectTypeTranslation> ProjectTypeTranslations { get; set; }
    
    public DbSet<ProjectStatus> ProjectStatuses { get; set; }
    public DbSet<ProjectStatusTranslation> ProjectStatusTranslations { get; set; }
    
    public DbSet<ProjectKind> ProjectKinds { get; set; }
    public DbSet<ProjectKindTranslation> ProjectKindTranslations { get; set; }
    
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<PermissionTranslation> PermissionTranslations { get; set; }
    #endregion
    
    public DbSet<WorkProject> WorkProjects { get; set; }
    public DbSet<WorkGroup> WorkGroups { get; set; }
    public DbSet<WorkTicket> WorkTickets { get; set; }
    public DbSet<WorkTask> WorkTasks { get; set; }
    
    public DbSet<WorkProjectParticipant> ProjectParticipants { get; set; }
    public DbSet<WorkProjectParticipantRole> ProjectParticipantRoles { get; set; }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Organization> Organizations { get; set; }
    
    public DbSet<Role> Roles { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    
    
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
        
        modelBuilder.Entity<WorkProject>(entity =>
        {
            entity.HasIndex(e => e.Title).IsUnique();
            entity.HasIndex(e => e.Code).IsUnique();
            
            entity.Property(e => e.Title).IsRequired();
            entity.Property(e => e.Code).IsRequired();

            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.CreatedById).IsRequired();

            entity.Property(u => u.ProjectTypeId)
                .HasDefaultValue(1)
                .IsRequired();
            
            entity.Property(u => u.ProjectKindId)
                .HasDefaultValue(1)
                .IsRequired();

            entity.Property(u => u.ProjectStatusId)
                .HasDefaultValue(1)
                .IsRequired();
            
            entity.HasOne(p => p.Organization)
                .WithMany(o => o.WorkProjects)
                .HasForeignKey(o => o.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasMany(p => p.WorkProjectParticipants)
                .WithOne(o => o.WorkProject)
                .HasForeignKey(o => o.WorkProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasMany(p => p.WorkGroups)
                .WithOne(o => o.WorkProject)
                .HasForeignKey(o => o.WorkProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<WorkGroup>(entity =>
        {
            entity.HasIndex(e => e.Title).IsUnique();
            
            entity.Property(e => e.Title).IsRequired();

            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.Level).IsRequired();
            
            
            entity.Property(u => u.StatusId)
                .HasDefaultValue(1)
                .IsRequired();
            
            
            entity.HasOne(e => e.ParentWorkGroup)
                .WithMany(e => e.Children)
                .HasForeignKey(e => e.ParentWorkGroupId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.WorkProject)
                .WithMany(o => o.WorkGroups)
                .HasForeignKey(e => e.WorkProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasIndex(e => e.Title).IsUnique();
            entity.HasIndex(e => e.Voen).IsUnique();
            
            entity.Property(e => e.Title).IsRequired();
            entity.Property(e => e.Voen).IsRequired();

            entity.Property(e => e.LogoPath).HasDefaultValue("logo path");
            
            
            entity.HasMany(o => o.Users)
                .WithOne(o => o.Organization)
                .HasForeignKey(o => o.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasMany(o => o.WorkProjects)
                .WithOne(o => o.Organization)
                .HasForeignKey(o => o.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        #region Dictionaries
        modelBuilder.Entity<ProjectStatus>(entity =>
        {
            entity.HasIndex(p => p.Code).IsUnique();
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();

            entity.HasMany(p => p.Translations)
                .WithOne(t => t.ProjectStatus)
                .HasForeignKey(t => t.ProjectStatusId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<ProjectStatusTranslation>(entity =>
        {
            entity.HasIndex(t => new { t.ProjectStatusId, t.Language }).IsUnique();
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
        
        modelBuilder.Entity<ProjectTypeTranslation>(entity =>
        {
            entity.HasIndex(t => new { t.ProjectTypeId, t.Language }).IsUnique();
            entity.Property(t => t.Language).HasMaxLength(5).IsRequired();
            entity.Property(t => t.Name).HasMaxLength(100).IsRequired();
        });
        
        
        modelBuilder.Entity<WorkGroupStatus>(entity =>
        {
            entity.HasIndex(p => p.Code).IsUnique();
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();

            entity.HasMany(p => p.Translations)
                .WithOne(t => t.WorkGroupStatus)
                .HasForeignKey(t => t.WorkGroupStatusId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<WorkGroupStatusTranslation>(entity =>
        {
            entity.HasIndex(t => new { t.WorkGroupStatusId, t.Language }).IsUnique();
            entity.Property(t => t.Language).HasMaxLength(5).IsRequired();
            entity.Property(t => t.Name).HasMaxLength(100).IsRequired();
        });
        
        
        modelBuilder.Entity<ProjectKind>(entity =>
        {
            entity.HasIndex(p => p.Code).IsUnique();
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();

            entity.HasMany(p => p.Translations)
                .WithOne(t => t.ProjectKind)
                .HasForeignKey(t => t.ProjectKindId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<ProjectKindTranslation>(entity =>
        {
            entity.HasIndex(t => new { t.ProjectKindId, t.Language }).IsUnique();
            entity.Property(t => t.Language).HasMaxLength(5).IsRequired();
            entity.Property(t => t.Name).HasMaxLength(100).IsRequired();
        });
        
        
        modelBuilder.Entity<WorkTaskStatus>(entity =>
        {
            entity.HasIndex(p => p.Code).IsUnique();
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();

            entity.HasMany(p => p.Translations)
                .WithOne(t => t.WorkTaskStatus)
                .HasForeignKey(t => t.WorkTaskStatusId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<WorkTaskStatusTranslation>(entity =>
        {
            entity.HasIndex(t => new { t.WorkTaskStatusId, t.Language }).IsUnique();
            entity.Property(t => t.Language).HasMaxLength(5).IsRequired();
            entity.Property(t => t.Name).HasMaxLength(100).IsRequired();
        });
        
        
        modelBuilder.Entity<WorkItemPriority>(entity =>
        {
            entity.HasIndex(p => p.Code).IsUnique();
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();

            entity.HasMany(p => p.Translations)
                .WithOne(t => t.WorkItemPriority)
                .HasForeignKey(t => t.WorkItemPriorityId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<WorkItemPriorityTranslation>(entity =>
        {
            entity.HasIndex(t => new { t.WorkItemPriorityId, t.Language }).IsUnique();
            entity.Property(t => t.Language).HasMaxLength(5).IsRequired();
            entity.Property(t => t.Name).HasMaxLength(100).IsRequired();
        });
        
        
        modelBuilder.Entity<WorkTicketStatus>(entity =>
        {
            entity.HasIndex(p => p.Code).IsUnique();
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();

            entity.HasMany(p => p.Translations)
                .WithOne(t => t.WorkTicketStatus)
                .HasForeignKey(t => t.WorkTicketStatusId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<WorkTicketStatusTranslation>(entity =>
        {
            entity.HasIndex(t => new { t.WorkTicketStatusId, t.Language }).IsUnique();
            entity.Property(t => t.Language).HasMaxLength(5).IsRequired();
            entity.Property(t => t.Name).HasMaxLength(100).IsRequired();
        });
        
        
        modelBuilder.Entity<WorkTicketType>(entity =>
        {
            entity.HasIndex(p => p.Code).IsUnique();
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();

            entity.HasMany(p => p.Translations)
                .WithOne(t => t.WorkTicketType)
                .HasForeignKey(t => t.WorkTicketTypeId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<WorkTicketTypeTranslation>(entity =>
        {
            entity.HasIndex(t => new { t.WorkTicketTypeId, t.Language }).IsUnique();
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
        
        modelBuilder.Entity<PermissionTranslation>(entity =>
        {
            entity.HasIndex(t => new { t.PermissionId, t.Language }).IsUnique();
            entity.Property(t => t.Language).HasMaxLength(5).IsRequired();
            entity.Property(t => t.Name).HasMaxLength(100).IsRequired();
        });
        #endregion
    }
}

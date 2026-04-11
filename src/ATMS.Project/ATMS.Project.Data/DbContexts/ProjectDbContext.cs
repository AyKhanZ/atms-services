using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Entities.Dictionaries;
using Microsoft.EntityFrameworkCore;

namespace ATMS.Project.Data.DbContexts;

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
    
    
    public DbSet<WorkProjectParticipant> WorkProjectParticipants { get; set; }
   
    public DbSet<WorkProjectParticipantRole> WorkProjectParticipantRoles { get; set; }
    
    
    public DbSet<User> Users { get; set; }
    
    public DbSet<Organization> Organizations { get; set; }
    
    
    public DbSet<Role> Roles { get; set; }
    
    public DbSet<RolePermission> RolePermissions { get; set; }
    
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5434;Database=atms_project;Username=admin;Password=p@ssw0rd!");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        #region Global Query Filters
        
        modelBuilder.Entity<WorkProject>()
            .HasQueryFilter(p => !p.IsDeleted);
    
        modelBuilder.Entity<Organization>()
            .HasQueryFilter(o => !o.IsDeleted);
    
        modelBuilder.Entity<WorkGroup>()
            .HasQueryFilter(t => !t.IsDeleted);
        
        modelBuilder.Entity<WorkTicket>()
            .HasQueryFilter(t => !t.IsDeleted);
        
        modelBuilder.Entity<WorkTask>()
            .HasQueryFilter(t => !t.IsDeleted);
        
        modelBuilder.Entity<User>()
            .HasQueryFilter(t => !t.IsDeleted);
        
        modelBuilder.Entity<Role>()
            .HasQueryFilter(t => !t.IsDeleted);
        
        modelBuilder.Entity<RolePermission>()
            .HasQueryFilter(rp => !rp.Role.IsDeleted);
        
        modelBuilder.Entity<WorkProjectParticipant>()
            .HasQueryFilter(t => !t.IsDeleted);
        
        modelBuilder.Entity<WorkProjectParticipantRole>()
            .HasQueryFilter(t => !t.IsDeleted);

        #endregion
        
        
        modelBuilder.Entity<WorkProject>(entity =>
        {
            entity.HasIndex(e => new { e.OrganizationId, e.Title }).IsUnique();
            
            entity.HasIndex(e => e.Title).IsUnique();
            entity.HasIndex(e => e.Code).IsUnique();
            
            entity.Property(e => e.Title).IsRequired();
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);

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
            
            entity.HasMany(p => p.WorkProjectParticipants)
                .WithOne(o => o.WorkProject)
                .HasForeignKey(o => o.WorkProjectId)
                .OnDelete(DeleteBehavior.NoAction);
            
            entity.HasMany(p => p.WorkGroups)
                .WithOne(o => o.WorkProject)
                .HasForeignKey(o => o.WorkProjectId)
                .OnDelete(DeleteBehavior.NoAction);
        });
        
        modelBuilder.Entity<WorkProjectParticipant>(entity =>
        {
            entity.HasIndex(e => new { e.WorkProjectId, e.UserId }).IsUnique();

            entity.HasOne(pp => pp.User)
                .WithMany()
                .HasForeignKey(pp => pp.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasMany(pp => pp.WorkProjectParticipantRoles)
                .WithOne(ppr => ppr.WorkProjectParticipant)
                .HasForeignKey(ppr => ppr.WorkProjectParticipantId)
                .OnDelete(DeleteBehavior.NoAction);
        });
        
        modelBuilder.Entity<WorkProjectParticipantRole>(entity =>
        {
            entity.HasIndex(e => new { e.WorkProjectParticipantId, e.RoleId }).IsUnique();

            entity.HasOne(ppr => ppr.Role)
                .WithMany(r => r.WorkProjectParticipantRoles)
                .HasForeignKey(ppr => ppr.RoleId)
                .OnDelete(DeleteBehavior.NoAction);
        });
        
        
        modelBuilder.Entity<WorkGroup>(entity =>
        {
            entity.HasIndex(e => new { e.WorkProjectId, e.ParentWorkGroupId, e.Title }).IsUnique();
            
            entity.HasIndex(e => e.Title).IsUnique();
            entity.HasIndex(e => e.Code).IsUnique();
            
            entity.Property(e => e.Title).IsRequired();
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);

            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.Level).IsRequired();
            
            
            entity.Property(u => u.StatusId)
                .HasDefaultValue(1)
                .IsRequired();
            
            entity.ToTable(t =>
                t.HasCheckConstraint("CK_WorkGroup_Level", "\"Level\" <= 1"));
            
            
            entity.HasOne(g => g.ParentWorkGroup)
                .WithMany(g => g.Children)
                .HasForeignKey(g => g.ParentWorkGroupId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasMany(g => g.WorkTickets)
                .WithOne(t => t.WorkGroup)
                .HasForeignKey(t => t.WorkGroupId)
                .OnDelete(DeleteBehavior.NoAction);
        });
        
        modelBuilder.Entity<WorkTicket>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();

            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);

            entity.Property(e => e.Title).IsRequired();

            entity.Property(e => e.WorkTicketStatusId)
                .HasDefaultValue(1)
                .IsRequired();

            entity.Property(e => e.WorkTicketTypeId).IsRequired();

            entity.Property(e => e.PriorityId)
                .HasDefaultValue(1)
                .IsRequired();

            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.CreatedById).IsRequired();

            entity.HasOne(t => t.WorkGroup)
                .WithMany(g => g.WorkTickets)
                .HasForeignKey(t => t.WorkGroupId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(t => t.Assignee)
                .WithMany()
                .HasForeignKey(t => t.AssigneeId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasMany(t => t.WorkTasks)
                .WithOne(wt => wt.WorkTicket)
                .HasForeignKey(wt => wt.WorkTicketId)
                .OnDelete(DeleteBehavior.NoAction);
        });
        
        modelBuilder.Entity<WorkTask>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();

            entity.Property(e => e.Code)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(300);

            entity.Property(e => e.Description)
                .HasMaxLength(4000);

            entity.Property(e => e.StatusId)
                .HasDefaultValue(1)
                .IsRequired();

            entity.Property(e => e.PriorityId)
                .HasDefaultValue(1)
                .IsRequired();

            entity.Property(e => e.Level).IsRequired();

            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.CreatedById).IsRequired();

            entity.ToTable(t =>
                t.HasCheckConstraint("CK_WorkTask_Level", "\"Level\" <= 1"));

            entity.HasOne(t => t.ParentWorkTask)
                .WithMany(t => t.Children)
                .HasForeignKey(t => t.ParentWorkTaskId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(t => t.Assignee)
                .WithMany()
                .HasForeignKey(t => t.AssigneeId)
                .OnDelete(DeleteBehavior.NoAction);
        });
        
        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasIndex(e => e.Title).IsUnique();
            entity.HasIndex(e => e.Voen).IsUnique();
            
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(e => e.Voen)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.LogoPath).HasDefaultValue("logo path");
            
            entity.HasMany(o => o.Users)
                .WithOne(u => u.Organization)
                .HasForeignKey(u => u.OrganizationId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasMany(o => o.WorkProjects)
                .WithOne(p => p.Organization)
                .HasForeignKey(p => p.OrganizationId)
                .OnDelete(DeleteBehavior.NoAction);
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
        
        
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();

            entity.Property(e => e.Email).IsRequired();
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Surname).IsRequired();
            entity.Property(e => e.UserTypeId).IsRequired();
        });
        
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).IsRequired();
        });
        
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(rp => new { rp.RoleId, rp.PermissionId });
    
            entity.HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        
            entity.HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

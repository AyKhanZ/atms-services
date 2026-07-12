using ATMS.Data.Interfaces;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Entities.Dictionaries;
using Microsoft.EntityFrameworkCore;

namespace ATMS.Project.Data.DbContexts;

public class ProjectDbContext : DbContext
{
    public ProjectDbContext() { }
    public ProjectDbContext(DbContextOptions<ProjectDbContext> options) : base(options) { }
    
    #region Dictionaries
    
    public DbSet<WorkTicketType> WorkTicketTypes { get; set; }
    
    public DbSet<WorkTicketStatus> WorkTicketStatuses { get; set; }
    
    public DbSet<WorkTaskStatus> WorkTaskStatuses { get; set; }
    
    public DbSet<WorkGroupStatus> WorkGroupStatuses { get; set; }
    
    public DbSet<WorkItemPriority> WorkItemPriorities { get; set; }
    
    public DbSet<ProjectType> ProjectTypes { get; set; }
    
    public DbSet<ProjectStatus> ProjectStatuses { get; set; }
    
    public DbSet<ProjectKind> ProjectKinds { get; set; }
    
    public DbSet<Permission> Permissions { get; set; }
    
    #endregion
    
    public DbSet<WorkProject> WorkProjects { get; set; }
    
    public DbSet<WorkGroup> WorkGroups { get; set; }
    
    public DbSet<WorkTicket> WorkTickets { get; set; }
    
    public DbSet<WorkTask> WorkTasks { get; set; }

    public DbSet<Comment> Comments { get; set; }

    public DbSet<Attachment> Attachments { get; set; }

    public DbSet<Meeting> Meetings { get; set; }

    public DbSet<MeetingParticipant> MeetingParticipants { get; set; }

    public DbSet<MeetingAgendaItem> MeetingAgendaItems { get; set; }

    public DbSet<MeetingMinute> MeetingMinutes { get; set; }
    
    
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
            optionsBuilder.UseNpgsql("Host=localhost;Port=5435;Database=atms_project;Username=admin;Password=p@ssw0rd!");
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<IAuditable>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
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

        modelBuilder.Entity<Comment>()
            .HasQueryFilter(t => !t.IsDeleted);

        modelBuilder.Entity<Attachment>()
            .HasQueryFilter(t => !t.IsDeleted);

        modelBuilder.Entity<Meeting>()
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
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProjectDbContext).Assembly);
    }
}

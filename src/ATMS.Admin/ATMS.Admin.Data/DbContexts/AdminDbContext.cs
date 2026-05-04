using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Entities.Dictionaries;
using ATMS.Admin.Data.Entities.Tokens;
using ATMS.Admin.Data.Entities.UserProgresses;
using ATMS.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ATMS.Admin.Data.DbContexts;

public class AdminDbContext: DbContext
{
    public AdminDbContext() { }
    public AdminDbContext(DbContextOptions<AdminDbContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; }
    
    public DbSet<Role> Roles { get; set; }

    public DbSet<UserRole> UserRoles { get; set; }
    
    public DbSet<RolePermission> RolePermissions { get; set; }
    
    #region Dictionaries
    public DbSet<Gender> Genders { get; set; }
    
    public DbSet<MaritalStatus> MaritalStatuses { get; set; }
    
    public DbSet<UserStatus> UserStatuses { get; set; }
    
    public DbSet<Permission> Permissions { get; set; }
    #endregion

    #region Tokens
    public DbSet<RefreshRevokedToken> RefreshRevokedTokens { get; set; }
    
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
    #endregion
    
    #region UserProgresses
    public DbSet<UserProgress> UserProgresses { get; set; }
    public DbSet<PersonalInfo> PersonalInfos { get; set; }
    public DbSet<InvitedUser> InvitedUsers { get; set; }
    #endregion

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=atms_admin;Username=admin;Password=p@ssw0rd!");
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
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AdminDbContext).Assembly);
    }
}

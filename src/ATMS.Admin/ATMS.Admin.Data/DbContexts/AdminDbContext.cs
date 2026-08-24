using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Entities.Dictionaries;
using ATMS.Admin.Data.Entities.Tokens;
using ATMS.Admin.Data.Entities.Onboarding;
using ATMS.Admin.Data.Entities.Messaging;
using ATMS.Data;
using ATMS.Data.Interfaces;
using ATMS.Data.Messaging;
using Microsoft.EntityFrameworkCore;

namespace ATMS.Admin.Data.DbContexts;

public class AdminDbContext: DbContext
{
    private readonly IAuditActorAccessor? _auditActor;

    public AdminDbContext() { }
    public AdminDbContext(DbContextOptions<AdminDbContext> options, IAuditActorAccessor? auditActor = null) : base(options)
    {
        _auditActor = auditActor;
    }

    public DbSet<User> Users { get; set; }
    
    public DbSet<Role> Roles { get; set; }

    public DbSet<UserRole> UserRoles { get; set; }
    
    public DbSet<RolePermission> RolePermissions { get; set; }
    
    public DbSet<Permission> Permissions { get; set; }
    
    #region Dictionaries
    public DbSet<Gender> Genders { get; set; }
    
    public DbSet<MaritalStatus> MaritalStatuses { get; set; }
    
    public DbSet<UserStatus> UserStatuses { get; set; }

    public DbSet<Language> Languages { get; set; }
    #endregion

    #region Tokens
    public DbSet<RefreshRevokedToken> RefreshRevokedTokens { get; set; }
    
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
    #endregion
    
    #region Onboarding
    public DbSet<OnboardingProgress> OnboardingProgresses { get; set; }

    public DbSet<OnboardingPersonalInfo> OnboardingPersonalInfos { get; set; }

    public DbSet<OnboardingInvitedUser> OnboardingInvitedUsers { get; set; }

    #endregion

    #region Messaging
    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    public DbSet<InboxMessage> InboxMessages { get; set; }

    public DbSet<EmailDelivery> EmailDeliveries { get; set; }

    #endregion

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5434;Database=atms_admin;Username=admin;Password=p@ssw0rd!");
        }
    }
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ChangeTracker.ApplyAuditMetadata(_auditActor?.UserId);

        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AdminDbContext).Assembly);
    }
}

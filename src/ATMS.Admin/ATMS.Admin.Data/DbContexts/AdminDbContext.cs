using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Entities.Dictionaries;
using ATMS.Admin.Data.Entities.Tokens;
using Microsoft.EntityFrameworkCore;

namespace ATMS.Admin.Data.DbContexts;

public class AdminDbContext: DbContext
{
    public AdminDbContext() { }
    public AdminDbContext(DbContextOptions<AdminDbContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }

    #region Dictionaries
    public DbSet<Gender> Genders { get; set; }
    public DbSet<MaritalStatus> MaritalStatuses { get; set; }
    public DbSet<UserStatus> UserStatuses { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    #endregion

    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<RefreshRevokedToken> RefreshRevokedTokens { get; set; }
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=atms;Username=admin;Password=p@ssw0rd!");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).IsRequired();

            entity.HasIndex(e => e.RefreshToken).IsUnique();

            entity.Property(e => e.AvatarPath).HasDefaultValue("test.png");

            entity.Property(u => u.MaritalStatusId)
                    .HasDefaultValue(1)
                    .IsRequired();

            entity.Property(u => u.UserStatusId)
                    .HasDefaultValue(1)
                    .IsRequired();

            entity.Property(u => u.GenderId)
                    .HasDefaultValue(1)
                    .IsRequired();
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).IsRequired();
        });

        #region Dictionaries
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasIndex(p => p.Code).IsUnique();

            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.Module)
                .HasMaxLength(50)
                .IsRequired();
        });

        modelBuilder.Entity<UserStatus>(entity =>
        {
            entity.HasIndex(p => p.Code).IsUnique();

            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .IsRequired();
        });

        modelBuilder.Entity<MaritalStatus>(entity =>
        {
            entity.HasIndex(p => p.Code).IsUnique();

            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .IsRequired();
        });

        modelBuilder.Entity<Gender>(entity =>
        {
            entity.HasIndex(p => p.Code).IsUnique();

            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .IsRequired();
        });
        #endregion

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(x => new { x.UserId, x.RoleId });

            entity.HasIndex(x => x.RoleId);
            entity.HasIndex(x => x.UserId);

            entity.HasOne(x => x.User)
                  .WithMany(x => x.UserRoles)
                  .HasForeignKey(x => x.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Role)
                  .WithMany(x => x.UserRoles)
                  .HasForeignKey(x => x.RoleId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(x => new { x.PermissionId, x.RoleId });

            entity.HasIndex(x => x.RoleId);
            entity.HasIndex(x => x.PermissionId);

            entity.HasOne(x => x.Permission)
                  .WithMany(x => x.RolePermissions)
                  .HasForeignKey(x => x.PermissionId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Role)
                  .WithMany(x => x.RolePermissions)
                  .HasForeignKey(x => x.RoleId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<RefreshRevokedToken>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Token).IsUnique();
        });
        
        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Token).IsUnique();
        });
    }
}
using ATMS.Admin.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ATMS.Admin.Data.DbContexts;

public class AdminDbContext: DbContext
{
    public AdminDbContext() { }
    public AdminDbContext(DbContextOptions<AdminDbContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }

    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }

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
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.Property(e => e.Name).IsRequired();
        });

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

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(x => new { x.UserId, x.RoleId });

            entity.HasIndex(x => x.RoleId);
            entity.HasIndex(x => x.UserId);

            entity.HasOne(x => x.User)
                  .WithMany(x => x.UserRoles)
                  .HasForeignKey(x => x.UserId);

            entity.HasOne(x => x.Role)
                  .WithMany(x => x.UserRoles)
                  .HasForeignKey(x => x.RoleId);
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(x => new { x.PermissionId, x.RoleId });

            entity.HasIndex(x => x.RoleId);
            entity.HasIndex(x => x.PermissionId);

            entity.HasOne(x => x.Permission)
                  .WithMany(x => x.RolePermissions)
                  .HasForeignKey(x => x.PermissionId);

            entity.HasOne(x => x.Role)
                  .WithMany(x => x.RolePermissions)
                  .HasForeignKey(x => x.RoleId);
        });
    }
}
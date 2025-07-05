using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SecureLoginApp.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace SecureLoginApp.DataAcces.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions options) : base(options)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);
    }

    public DbSet<User> Users { get; set; }
    public DbSet<UserOTPs> UserOTPs { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<PermissionGroup> PermissionGroups { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {

        base.OnModelCreating(builder);
        // ⚠️ Agar kerakli `OnDelete` yoki `HasKey`, `HasIndex` lar bo‘lsa, shu yerga yoziladi

        // RolePermission - ko‘p-ko‘p
        builder.Entity<RolePermission>()
            .HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<RolePermission>()
            .HasOne(rp => rp.Permission)
            .WithMany(p => p.Roles)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        // UserRole - ko‘p-ko‘p
        builder.Entity<UserRole>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserRole>()
            .HasOne(ur => ur.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        // PermissionGroup - Permission bilan 1-ko‘p
        builder.Entity<Permission>()
            .HasOne(p => p.PermissionGroup)
            .WithMany(pg => pg.Permissions)
            .HasForeignKey(p => p.PermissionGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        // UserOTPs - User bilan 1-ko‘p
        builder.Entity<UserOTPs>()
            .HasOne(uo => uo.User)
            .WithMany() // yoki .WithMany(u => u.OtpCodes) agar navigation bo‘lsa
            .HasForeignKey(uo => uo.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Avtomatik konfiguratsiyalar (agar konfiguratsiyalar alohida fayllarda bo‘lsa)
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly); // Hozirgi DataAccess assemblysi


    }
}

using MasterApp.Auth.Application.Interfaces;
using MasterApp.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MasterApp.Auth.Infrastructure.Data;

public class AuthDbContext : DbContext, IAuthDbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Device> Devices { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<ActivationCode> ActivationCodes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.HasDefaultSchema("Auth");
        modelBuilder.HasPostgresEnum<RoleType>(schema: "Auth", name: "RoleType");

        modelBuilder.Entity<User>(entity => 
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Phone).IsUnique();
            entity.HasIndex(e => e.Login).IsUnique();
            entity.Property(e => e.Role).IsRequired();
            
            entity.HasMany(e => e.RefreshTokens).WithOne(e => e.User).HasForeignKey(e => e.UserId);
        });

        modelBuilder.Entity<Device>(entity => 
        {
            entity.ToTable("Devices");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DeviceId).IsRequired();
        });

        modelBuilder.Entity<RefreshToken>(entity => 
        {
            entity.ToTable("RefreshTokens");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Token).IsRequired();
        });

        modelBuilder.Entity<ActivationCode>(entity => 
        {
            entity.ToTable("ActivationCodes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired();
        });
    }
}

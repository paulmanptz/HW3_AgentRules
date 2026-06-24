using MasterApp.Application.Interfaces;
using MasterApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MasterApp.Infrastructure.Data;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }


    public DbSet<Organization> Organizations { get; set; } = null!;
    public DbSet<OrganizationMaster> OrganizationMasters { get; set; } = null!;
    public DbSet<OrganizationDispatcher> OrganizationDispatchers { get; set; } = null!;
    public DbSet<Service> Services { get; set; } = null!;
    public DbSet<ServiceQuest> ServiceQuests { get; set; } = null!;
    public DbSet<ServiceQuestStatusHistory> ServiceQuestStatusHistories { get; set; } = null!;
    public DbSet<ServiceQuestAttachment> ServiceQuestAttachments { get; set; } = null!;
    public DbSet<MasterNotification> MasterNotifications { get; set; } = null!;
    public DbSet<ClientNotification> ClientNotifications { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        modelBuilder.Entity<Organization>().HasKey(o => o.Id);
        modelBuilder.Entity<Service>().HasKey(s => s.Id);

        modelBuilder.Entity<OrganizationMaster>()
            .HasKey(om => new { om.OrgId, om.UserId });

        modelBuilder.Entity<OrganizationMaster>()
            .HasOne(om => om.Organization)
            .WithMany(o => o.OrganizationMasters)
            .HasForeignKey(om => om.OrgId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OrganizationDispatcher>()
            .HasKey(od => od.UserId);

        modelBuilder.Entity<OrganizationDispatcher>()
            .HasOne(od => od.Organization)
            .WithMany()
            .HasForeignKey(od => od.OrgId)
            .OnDelete(DeleteBehavior.Cascade);


        
        modelBuilder.Entity<ServiceQuest>()
            .HasKey(sr => sr.Id);



        modelBuilder.Entity<ServiceQuest>()
            .HasOne(sr => sr.Service)
            .WithMany()
            .HasForeignKey(sr => sr.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ServiceQuestStatusHistory>()
            .HasKey(sh => sh.Id);

        modelBuilder.Entity<ServiceQuestStatusHistory>()
            .HasOne(sh => sh.ServiceQuest)
            .WithMany(sr => sr.StatusHistory)
            .HasForeignKey(sh => sh.StatusRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ServiceQuestAttachment>()
            .HasKey(sa => sa.Id);

        modelBuilder.Entity<ServiceQuestAttachment>()
            .HasOne(sa => sa.ServiceQuest)
            .WithMany(sr => sr.Attachments)
            .HasForeignKey(sa => sa.ServiceQuestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

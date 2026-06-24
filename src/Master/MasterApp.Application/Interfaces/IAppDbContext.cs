using MasterApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MasterApp.Application.Interfaces;

public interface IAppDbContext
{

    DbSet<Organization> Organizations { get; }
    DbSet<OrganizationMaster> OrganizationMasters { get; }
    DbSet<OrganizationDispatcher> OrganizationDispatchers { get; }
    DbSet<Service> Services { get; }
    DbSet<ServiceQuest> ServiceQuests { get; }
    DbSet<ServiceQuestStatusHistory> ServiceQuestStatusHistories { get; }
    DbSet<ServiceQuestAttachment> ServiceQuestAttachments { get; }
    DbSet<MasterNotification> MasterNotifications { get; }
    DbSet<ClientNotification> ClientNotifications { get; }
    Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade Database { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

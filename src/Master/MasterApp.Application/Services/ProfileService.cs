using System;
using System.Threading;
using System.Threading.Tasks;
using MasterApp.Application.DTOs;
using MasterApp.Application.Interfaces;
using MasterApp.Auth.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using MasterApp.Domain.Enums;

namespace MasterApp.Application.Services;

public class ProfileService : IProfileService
{
    private readonly IAppDbContext _appContext;
    private readonly IAuthDbContext _authContext;

    public ProfileService(IAppDbContext appContext, IAuthDbContext authContext)
    {
        _appContext = appContext;
        _authContext = authContext;
    }

    public async Task<MasterProfileDto> GetProfileAsync(Guid masterId, CancellationToken cancellationToken)
    {
        var master = await _authContext.Users
            .FirstOrDefaultAsync(m => m.Id == masterId, cancellationToken);
            
        if (master == null) throw new Exception("Master not found");

        var organizationMasters = await _appContext.OrganizationMasters
            .Include(om => om.Organization)
            .Where(om => om.UserId == masterId)
            .ToListAsync(cancellationToken);

        var newCount = await _appContext.ServiceQuests.CountAsync(r => r.MasterId == masterId && r.Status == ServiceQuestStatus.New, cancellationToken);
        var inProgressCount = await _appContext.ServiceQuests.CountAsync(r => r.MasterId == masterId && r.Status == ServiceQuestStatus.InProgress, cancellationToken);
        var completedCount = await _appContext.ServiceQuests.CountAsync(r => r.MasterId == masterId && r.Status == ServiceQuestStatus.Completed, cancellationToken);

        return new MasterProfileDto
        {
            Id = master.Id,
            Name = $"{master.FirstName} {master.LastName}".Trim(),
            Specialization = string.Empty,
            OrganizationName = organizationMasters.Any() 
                ? string.Join(", ", organizationMasters.Select(om => om.Organization?.Name ?? $"Организация {om.OrgId}"))
                : "Нет организации",
            NewRequestsCount = newCount,
            InProgressRequestsCount = inProgressCount,
            CompletedRequestsCount = completedCount
        };
    }
}

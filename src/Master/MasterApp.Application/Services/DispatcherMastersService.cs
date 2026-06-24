using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MasterApp.Application.Interfaces;
using MasterApp.Auth.Application.Interfaces;
using MasterApp.Domain.Entities;
using MasterApp.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MasterApp.Application.Services;

public class DispatcherMastersService : IDispatcherMastersService
{
    private readonly IAppDbContext _appContext;
    private readonly IAuthDbContext _authContext;

    public DispatcherMastersService(IAppDbContext appContext, IAuthDbContext authContext)
    {
        _appContext = appContext;
        _authContext = authContext;
    }

    public async Task<User> CreateMasterAsync(int orgId, CreateMasterRequest request, CancellationToken cancellationToken)
    {
        var targetRole = RoleType.Master;
        var existingMaster = await _authContext.Users
            .FirstOrDefaultAsync(m => m.Phone == request.Phone && m.Role == targetRole, cancellationToken);

        if (existingMaster != null)
        {
            throw new Exception("Мастер с таким номером телефона уже существует.");
        }

        var master = new User
        {
            Id = Guid.NewGuid(),
            LastName = request.LastName,
            FirstName = request.FirstName,
            Patronymic = request.Patronymic,
            Phone = request.Phone,
            Role = RoleType.Master,
            CreatedAt = DateTime.UtcNow
        };

        _authContext.Users.Add(master);
        await _authContext.SaveChangesAsync(cancellationToken);

        _appContext.OrganizationMasters.Add(new OrganizationMaster { OrgId = orgId, UserId = master.Id });
        await _appContext.SaveChangesAsync(cancellationToken);

        return master;
    }

    public async Task<List<MasterDto>> GetMastersAsync(int orgId, CancellationToken cancellationToken)
    {
        var orgUserIds = await _appContext.OrganizationMasters
            .Where(om => om.OrgId == orgId)
            .Select(om => om.UserId)
            .ToListAsync(cancellationToken);

        var targetRole = RoleType.Master;
        return await _authContext.Users
            .Where(m => m.Role == targetRole && orgUserIds.Contains(m.Id))
            .Select(m => new MasterDto
            {
                Id = m.Id,
                Phone = m.Phone,
                FirstName = m.FirstName,
                LastName = m.LastName,
                Patronymic = m.Patronymic,
                IsActive = m.IsActive,
                UpdatedAt = m.UpdatedAt,
                CreatedAt = m.CreatedAt,
                DeletedAt = m.DeletedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<User> UpdateMasterAsync(int orgId, Guid masterId, UpdateMasterRequest request, CancellationToken cancellationToken)
    {
        var belongsToOrg = await _appContext.OrganizationMasters
            .AnyAsync(om => om.OrgId == orgId && om.UserId == masterId, cancellationToken);
        if (!belongsToOrg)
            throw new Exception("Мастер не найден в вашей организации.");

        var master = await _authContext.Users
            .FirstOrDefaultAsync(u => u.Id == masterId && u.Role == RoleType.Master, cancellationToken);
        if (master == null)
            throw new Exception("Мастер не найден.");

        var duplicatePhone = await _authContext.Users
            .AnyAsync(u => u.Phone == request.Phone && u.Id != masterId && u.Role == RoleType.Master, cancellationToken);
        if (duplicatePhone)
            throw new Exception("Мастер с таким номером телефона уже существует.");

        master.LastName = request.LastName;
        master.FirstName = request.FirstName;
        master.Patronymic = request.Patronymic;
        master.Phone = request.Phone;
        master.IsActive = request.IsActive;
        master.UpdatedAt = DateTime.UtcNow;

        await _authContext.SaveChangesAsync(cancellationToken);
        return master;
    }
}

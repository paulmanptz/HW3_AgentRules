using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MasterApp.Auth.Application.Interfaces;
using MasterApp.Auth.Domain.Entities;
using MasterApp.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MasterApp.Application.Services;

public class UserClaimsProvider : IUserClaimsProvider
{
    private readonly IAppDbContext _appDbContext;

    public UserClaimsProvider(IAppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<IDictionary<string, string>> GetAdditionalClaimsAsync(User user, CancellationToken cancellationToken)
    {
        var claims = new Dictionary<string, string>();

        if (user.Role == RoleType.Dispatcher)
        {
            var orgDispatcher = await _appDbContext.OrganizationDispatchers
                .FirstOrDefaultAsync(od => od.UserId == user.Id, cancellationToken);
                
            if (orgDispatcher != null)
            {
                claims.Add("OrgId", orgDispatcher.OrgId.ToString());
            }
            else
            {
                // Fallback and auto-migrate legacy dispatcher org mappings
                var legacyLink = await _appDbContext.OrganizationMasters
                    .FirstOrDefaultAsync(ou => ou.UserId == user.Id, cancellationToken);
                
                if (legacyLink != null)
                {
                    claims.Add("OrgId", legacyLink.OrgId.ToString());
                    _appDbContext.OrganizationDispatchers.Add(new MasterApp.Domain.Entities.OrganizationDispatcher 
                    { 
                        UserId = user.Id, 
                        OrgId = legacyLink.OrgId 
                    });
                    try { await _appDbContext.SaveChangesAsync(cancellationToken); } catch { }
                }
            }
        }

        return claims;
    }
}

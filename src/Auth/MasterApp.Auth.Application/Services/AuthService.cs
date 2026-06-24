using System;
using System.Threading;
using System.Threading.Tasks;
using MasterApp.Auth.Application.DTOs;
using MasterApp.Auth.Application.Interfaces;

using MasterApp.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MasterApp.Auth.Application.Services;

public class AuthService : IAuthService
{
    private readonly IAuthDbContext _context;
    private readonly IJwtProvider _jwtProvider;
    private readonly System.Collections.Generic.IEnumerable<IUserClaimsProvider> _claimsProviders;

    public AuthService(IAuthDbContext context, IJwtProvider jwtProvider, System.Collections.Generic.IEnumerable<IUserClaimsProvider> claimsProviders)
    {
        _context = context;
        _jwtProvider = jwtProvider;
        _claimsProviders = claimsProviders;
    }

    public Task SendCodeAsync(string phoneNumber, CancellationToken cancellationToken)
    {
        // Stub: do nothing or log
        return Task.CompletedTask;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        // Stub verification: accept any code for MVP
        var targetRole = RoleType.Master;
        var master = await _context.Users
            .FirstOrDefaultAsync(m => m.Phone == request.PhoneNumber && m.Role == targetRole, cancellationToken);
        
        if (master == null)
        {
            throw new Exception("Учетная запись с таким номером телефона не найдена.");
        }

        if (request.DeviceId != null)
        {
            var device = await _context.Devices.FirstOrDefaultAsync(d => d.DeviceId == request.DeviceId && d.UserId == master.Id, cancellationToken);
            if (device == null)
            {
                device = new Device { Id = Guid.NewGuid(), UserId = master.Id, DeviceId = request.DeviceId, CreatedAt = DateTime.UtcNow };
                _context.Devices.Add(device);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        var customClaims = new System.Collections.Generic.Dictionary<string, string>();
        foreach (var provider in _claimsProviders)
        {
            var pClaims = await provider.GetAdditionalClaimsAsync(master, cancellationToken);
            foreach (var kvp in pClaims) customClaims[kvp.Key] = kvp.Value;
        }

        var token = _jwtProvider.GenerateToken(master, customClaims);

        return new AuthResponse
        {
            Token = token,
            UserId = master.Id
        };
    }
}
